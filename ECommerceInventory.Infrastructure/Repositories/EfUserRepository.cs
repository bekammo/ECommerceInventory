using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceInventory.Infrastructure.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public EfUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
