using ECommerceInventory.Domain.Entities;

namespace ECommerceInventory.Application.Interfaces.Repositories;

public interface IOutboxRepository
{
    Task<IEnumerable<OutboxEvent>> GetPendingAsync();
    Task AddAsync(OutboxEvent outboxEvent);
    Task UpdateAsync(OutboxEvent outboxEvent);
}
