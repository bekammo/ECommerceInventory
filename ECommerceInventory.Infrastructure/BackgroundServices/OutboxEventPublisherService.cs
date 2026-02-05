using ECommerceInventory.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceInventory.Infrastructure.BackgroundServices;

public class OutboxEventPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxEventPublisherService> _logger;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetries = 5;

    public OutboxEventPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxEventPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Event Publisher Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Outbox Event Publisher Service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox events.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Event Publisher Service stopped.");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        var pendingEvents = await outboxRepo.GetPendingAsync();

        if (!pendingEvents.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} pending outbox events.", pendingEvents.Count());

        foreach (var outboxEvent in pendingEvents)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                _logger.LogInformation(
                    "Publishing event {EventId} of type {EventType}: {Payload}",
                    outboxEvent.Id,
                    outboxEvent.EventType,
                    outboxEvent.Payload);

                outboxEvent.Status = "Completed";
                outboxEvent.ProcessedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Event {EventId} published successfully.",
                    outboxEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to publish event {EventId}. Retry count: {RetryCount}",
                    outboxEvent.Id,
                    outboxEvent.RetryCount);

                outboxEvent.RetryCount++;

                if (outboxEvent.RetryCount >= MaxRetries)
                {
                    outboxEvent.Status = "Failed";
                    _logger.LogError(
                        "Event {EventId} exceeded max retries ({MaxRetries}). Marked as failed.",
                        outboxEvent.Id,
                        MaxRetries);
                }
            }

            await outboxRepo.UpdateAsync(outboxEvent);
        }
    }
}
