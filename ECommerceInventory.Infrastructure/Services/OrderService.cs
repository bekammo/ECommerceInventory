using System.Text.Json;
using ECommerceInventory.Application.DTOs.Common;
using ECommerceInventory.Application.DTOs.Order;
using ECommerceInventory.Application.Interfaces;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Discounts;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Domain.Enums;
using ECommerceInventory.Infrastructure.BackgroundServices;
using ECommerceInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerceInventory.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly PaymentQueue _paymentQueue;
    private readonly DiscountCardFactory _discountFactory;
    private readonly ILogger<OrderService> _logger;
    private const int MaxRetries = 3;

    public OrderService(
        ApplicationDbContext context,
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        PaymentQueue paymentQueue,
        DiscountCardFactory discountFactory,
        ILogger<OrderService> logger)
    {
        _context = context;
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _paymentQueue = paymentQueue;
        _discountFactory = discountFactory;
        _logger = logger;
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return ApiResponse<OrderDto>.FailureResponse("Order must contain at least one item.");
        }

        // Validate all items have valid product IDs
        if (request.Items.Any(i => i.ProductId == Guid.Empty))
        {
            return ApiResponse<OrderDto>.FailureResponse("Invalid product ID in order items.");
        }

        // Check for duplicate products
        var hasDuplicates = request.Items
            .GroupBy(i => i.ProductId)
            .Any(g => g.Count() > 1);

        if (hasDuplicates)
        {
            return ApiResponse<OrderDto>.FailureResponse(
                "Order contains duplicate products. Please combine quantities for the same product.");
        }

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var products = new Dictionary<Guid, Product>();
                foreach (var item in request.Items)
                {
                    Product? product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product is null)
                    {
                        await transaction.RollbackAsync();
                        _context.ChangeTracker.Clear();
                        return ApiResponse<OrderDto>.FailureResponse("Product not found.");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        _context.ChangeTracker.Clear();
                        return ApiResponse<OrderDto>.FailureResponse(
                            $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity}");
                    }

                    products[item.ProductId] = product;
                }

                foreach (var item in request.Items)
                {
                    var product = products[item.ProductId];
                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);
                }
                await _context.SaveChangesAsync();

                var items = new List<OrderItem>();
                decimal total = 0;

                foreach (var item in request.Items)
                {
                    var product = products[item.ProductId];
                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * item.Quantity
                    };
                    items.Add(orderItem);
                    total += orderItem.TotalPrice;
                }

                decimal discount = 0;
                if (!string.IsNullOrWhiteSpace(request.DiscountCode))
                {
                    discount = ApplyDiscount(request.DiscountCode, total);
                }

                var final = total - discount;

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Items = items,
                    TotalAmount = total,
                    DiscountAmount = discount,
                    FinalAmount = final,
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var item in items)
                {
                    item.OrderId = order.Id;
                }

                await _context.Orders.AddAsync(order);

                var outboxEvent = new OutboxEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = "OrderCreated",
                    Payload = JsonSerializer.Serialize(new { OrderId = order.Id, order.FinalAmount }),
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending",
                    RetryCount = 0
                };
                await _context.OutboxEvents.AddAsync(outboxEvent);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await QueuePaymentEventAsync(order);

                return ApiResponse<OrderDto>.SuccessResponse(MapToDto(order));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();

                if (attempt == MaxRetries - 1)
                {
                    return ApiResponse<OrderDto>.FailureResponse(
                        "Unable to complete order due to concurrent modifications. Please try again.");
                }

                await Task.Delay(100 * (attempt + 1));
            }
        }

        return ApiResponse<OrderDto>.FailureResponse("Unable to complete order. Please try again.");
    }

    public async Task<ApiResponse<IEnumerable<OrderDto>>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _orderRepo.GetByUserIdAsync(userId);
        var dtos = orders.Select(MapToDto);
        return ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId)
        {
            return ApiResponse<OrderDto>.FailureResponse("Order not found.");
        }

        return ApiResponse<OrderDto>.SuccessResponse(MapToDto(order));
    }

    public async Task<ApiResponse<OrderStatusDto>> GetOrderStatusAsync(Guid userId, Guid orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId)
        {
            return ApiResponse<OrderStatusDto>.FailureResponse("Order not found.");
        }

        var dto = new OrderStatusDto(order.Id, order.Status, order.PaymentStatus);
        return ApiResponse<OrderStatusDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<bool>> UpdatePaymentStatusAsync(Guid orderId, PaymentStatus status)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null)
        {
            return ApiResponse<bool>.FailureResponse("Order not found.");
        }

        order.PaymentStatus = status;

        if (status == PaymentStatus.Completed)
        {
            order.Status = OrderStatus.Processing;
        }
        else if (status == PaymentStatus.Failed)
        {
            order.Status = OrderStatus.Failed;
        }

        await _orderRepo.UpdateAsync(order);

        return ApiResponse<bool>.SuccessResponse(true);
    }

    private decimal ApplyDiscount(string code, decimal total)
    {
        try
        {
            var (cardType, value, minAmount) = ParseDiscountCode(code);
            var card = _discountFactory.CreateCard(cardType, value, minAmount);

            if (card.CanApply(total))
            {
                return card.CalculateDiscount(total);
            }
            
            _logger.LogWarning("Discount code {DiscountCode} does not meet minimum amount requirement. Total: {Total}, MinAmount: {MinAmount}", 
                code, total, minAmount);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid discount code format: {DiscountCode}", code);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Failed to parse discount code: {DiscountCode}", code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error applying discount code: {DiscountCode}", code);
        }

        return 0;
    }

    private static (string cardType, decimal value, decimal minAmount) ParseDiscountCode(string code)
    {
        var parts = code.Split('-');
        if (parts.Length < 2)
        {
            throw new ArgumentException("Invalid discount code format.");
        }

        var cardType = parts[0];
        var value = decimal.Parse(parts[1]);
        var minAmount = parts.Length > 2 ? decimal.Parse(parts[2]) : 0;

        return (cardType, value, minAmount);
    }

    private async Task QueuePaymentEventAsync(Order order)
    {
        try
        {
            var paymentTask = new PaymentTask(order.Id, order.FinalAmount);
            await _paymentQueue.EnqueueAsync(paymentTask);
            _logger.LogInformation("Payment queued for order {OrderId}, amount: {Amount}", order.Id, order.FinalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue payment for order {OrderId}. Payment processing may be delayed.", order.Id);
            // Note: Order is still created successfully, but payment processing might need manual intervention
        }
    }

    private static OrderDto MapToDto(Order order)
    {
        var items = order.Items.Select(i => new OrderItemDto(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice)).ToList();

        return new OrderDto(
            order.Id,
            order.UserId,
            items,
            order.TotalAmount,
            order.DiscountAmount,
            order.FinalAmount,
            order.Status,
            order.PaymentStatus,
            order.CreatedAt);
    }
}
