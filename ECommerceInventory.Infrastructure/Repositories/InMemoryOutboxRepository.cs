using System.Collections.Concurrent;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;

namespace ECommerceInventory.Infrastructure.Repositories;

public class InMemoryOutboxRepository : IOutboxRepository
{
    private readonly ConcurrentDictionary<Guid, OutboxEvent> _outboxEvents = new();

    public Task<IEnumerable<OutboxEvent>> GetPendingAsync()
    {
        var pendingEvents = _outboxEvents.Values
            .Where(e => e.ProcessedAt == null && e.Status == "Pending")
            .OrderBy(e => e.CreatedAt);
        return Task.FromResult(pendingEvents.AsEnumerable());
    }

    public Task AddAsync(OutboxEvent outboxEvent)
    {
        outboxEvent.Status = "Pending";
        if (!_outboxEvents.TryAdd(outboxEvent.Id, outboxEvent))
        {
            throw new InvalidOperationException($"OutboxEvent with ID {outboxEvent.Id} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(OutboxEvent outboxEvent)
    {
        if (!_outboxEvents.ContainsKey(outboxEvent.Id))
        {
            throw new InvalidOperationException($"OutboxEvent with ID {outboxEvent.Id} not found.");
        }
        _outboxEvents[outboxEvent.Id] = outboxEvent;
        return Task.CompletedTask;
    }
}
