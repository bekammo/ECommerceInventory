using ECommerceInventory.Application.DTOs.Order;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Discounts;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.BackgroundServices;
using ECommerceInventory.Infrastructure.Data;
using ECommerceInventory.Infrastructure.Repositories;
using ECommerceInventory.Infrastructure.Services;
using ECommerceInventory.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerceInventory.UnitTests.Services;

public class OrderServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly PaymentQueue _paymentQueue;
    private readonly DiscountCardFactory _discountCardFactory;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _context = MockSetup.CreateInMemoryDbContext();
        _orderRepository = new EfOrderRepository(_context);
        _productRepository = new EfProductRepository(_context);
        _paymentQueue = new PaymentQueue();
        _discountCardFactory = new DiscountCardFactory();

        _orderService = new OrderService(
            _context,
            _orderRepository,
            _productRepository,
            _paymentQueue,
            _discountCardFactory,
            Mock.Of<ILogger<OrderService>>());
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateOrder_WithStock_Success()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 25m,
            StockQuantity = 100,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 2)],
            null);

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalAmount.Should().Be(50m);
        result.Data.FinalAmount.Should().Be(50m);

        var orders = await _context.Orders.ToListAsync();
        orders.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateOrder_OutOfStock_Fails()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 25m,
            StockQuantity = 5,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 10)],
            null);

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Insufficient stock");

        var orders = await _context.Orders.ToListAsync();
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrder_ProductNotFound_Fails()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 1)],
            null);

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");

        var orders = await _context.Orders.ToListAsync();
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrder_AppliesPercentageDiscount()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 100m,
            StockQuantity = 50,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 1)],
            "PERCENTAGE-10-0"); // 10% discount, no minimum

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalAmount.Should().Be(100m);
        result.Data.DiscountAmount.Should().Be(10m);
        result.Data.FinalAmount.Should().Be(90m);
    }

    [Fact]
    public async Task CreateOrder_AppliesFixedDiscount_WhenAboveMinimum()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 150m,
            StockQuantity = 50,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 1)],
            "FIXEDAMOUNT-20-100"); // $20 off, minimum $100

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalAmount.Should().Be(150m);
        result.Data.DiscountAmount.Should().Be(20m);
        result.Data.FinalAmount.Should().Be(130m);
    }

    [Fact]
    public async Task CreateOrder_SkipsFixedDiscount_WhenBelowMinimum()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 50m,
            StockQuantity = 50,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 1)],
            "FIXEDAMOUNT-20-100"); 

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalAmount.Should().Be(50m);
        result.Data.DiscountAmount.Should().Be(0m);
        result.Data.FinalAmount.Should().Be(50m);
    }

    [Fact]
    public async Task CreateOrder_CreatesOutboxEvent()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 25m,
            StockQuantity = 100,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 2)],
            null);

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeTrue();

        var outboxEvents = await _context.OutboxEvents.ToListAsync();
        outboxEvents.Should().HaveCount(1);
        outboxEvents[0].EventType.Should().Be("OrderCreated");
        outboxEvents[0].Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CreateOrder_QueuesPayment()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 25m,
            StockQuantity = 100,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 2)],
            null);

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeTrue();

        var hasPayment = _paymentQueue.Reader.TryRead(out var paymentTask);
        hasPayment.Should().BeTrue();
        paymentTask.Should().NotBeNull();
        paymentTask!.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task CreateOrder_DecrementsStock()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Description",
            Price = 25m,
            StockQuantity = 100,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            [new OrderItemRequest(productId, 10)],
            null);

        var result = await _orderService.CreateOrderAsync(userId, request);


        result.Success.Should().BeTrue();

        var updatedProduct = await _context.Products.FindAsync(productId);
        updatedProduct!.StockQuantity.Should().Be(90);
    }
}
