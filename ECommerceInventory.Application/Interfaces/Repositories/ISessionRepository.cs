using ECommerceInventory.Domain.Entities;

namespace ECommerceInventory.Application.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id);
    Task<Session?> GetByTokenAsync(string token);
    Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Session>> GetActiveByUserIdAsync(Guid userId);
    Task AddAsync(Session session);
    Task UpdateAsync(Session session);
    Task RevokeAllByUserIdAsync(Guid userId);
}
