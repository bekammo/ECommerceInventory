using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceInventory.Infrastructure.Repositories;

public class EfSessionRepository : ISessionRepository
{
    private readonly ApplicationDbContext _context;

    public EfSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetByIdAsync(Guid id)
    {
        return await _context.Sessions.FindAsync(id);
    }

    public async Task<Session?> GetByTokenAsync(string token)
    {
        return await _context.Sessions.FirstOrDefaultAsync(s => s.Token == token);
    }

    public async Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetActiveByUserIdAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _context.Sessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > now)
            .ToListAsync();
    }

    public async Task AddAsync(Session session)
    {
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Session session)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllByUserIdAsync(Guid userId)
    {
        await _context.Sessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRevoked, true));
    }
}
