using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceInventory.Infrastructure.Repositories;

public class EfOutboxRepository : IOutboxRepository
{
    private readonly ApplicationDbContext _context;

    public EfOutboxRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OutboxEvent>> GetPendingAsync()
    {
        return await _context.OutboxEvents
            .Where(e => e.ProcessedAt == null)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(OutboxEvent outboxEvent)
    {
        await _context.OutboxEvents.AddAsync(outboxEvent);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(OutboxEvent outboxEvent)
    {
        _context.OutboxEvents.Update(outboxEvent);
        await _context.SaveChangesAsync();
    }
}
