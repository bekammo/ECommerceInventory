using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceInventory.Infrastructure.Repositories;

public class EfOrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public EfOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
}
