using System.Text.Json;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceInventory.Infrastructure.BackgroundServices;

public class PaymentProcessingService : BackgroundService
{
    private readonly PaymentQueue _paymentQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentProcessingService> _logger;
    private static readonly TimeSpan PaymentProcessingDelay = TimeSpan.FromMinutes(2);

    public PaymentProcessingService(
        PaymentQueue paymentQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentProcessingService> logger)
    {
        _paymentQueue = paymentQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Processing Service started.");

        await foreach (var paymentTask in _paymentQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessPaymentAsync(paymentTask, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Payment Processing Service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order {OrderId}", paymentTask.OrderId);
            }
        }

        _logger.LogInformation("Payment Processing Service stopped.");
    }

    private async Task ProcessPaymentAsync(PaymentTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing payment for order {OrderId}, amount: {Amount}",
            task.OrderId,
            task.Amount);

        using var scope = _scopeFactory.CreateScope();
        var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        var order = await orderRepo.GetByIdAsync(task.OrderId);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found for payment processing.", task.OrderId);
            return;
        }

        order.PaymentStatus = PaymentStatus.Processing;
        await orderRepo.UpdateAsync(order);

        await Task.Delay(PaymentProcessingDelay, cancellationToken);

        var success = Random.Shared.NextDouble() > 0.1;

        if (success)
        {
            order.PaymentStatus = PaymentStatus.Completed;
            order.Status = OrderStatus.Processing;

            _logger.LogInformation("Payment completed successfully for order {OrderId}", task.OrderId);

            await AddOrderCompletedEventAsync(order, outboxRepo);
        }
        else
        {
            order.PaymentStatus = PaymentStatus.Failed;
            order.Status = OrderStatus.Failed;

            _logger.LogWarning("Payment failed for order {OrderId}", task.OrderId);
        }

        await orderRepo.UpdateAsync(order);
    }

    private async Task AddOrderCompletedEventAsync(Order order, IOutboxRepository outboxRepo)
    {
        var evt = new
        {
            OrderId = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            ItemCount = order.Items.Count,
            CompletedAt = DateTime.UtcNow
        };

        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "OrderCompleted",
            Payload = JsonSerializer.Serialize(evt),
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0,
            Status = "Pending"
        };

        await outboxRepo.AddAsync(outboxEvent);

        _logger.LogInformation(
            "OrderCompleted event added to outbox for order {OrderId}",
            order.Id);
    }
}
