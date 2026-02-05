using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Domain.Exceptions;
using ECommerceInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceInventory.Infrastructure.Repositories;

public class EfProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public EfProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("A concurrency conflict occurred while adding the product.", ex);
        }
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("A concurrency conflict occurred while updating the product.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is not null)
        {
            _context.Products.Remove(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("A concurrency conflict occurred while deleting the product.", ex);
            }
        }
    }
}
