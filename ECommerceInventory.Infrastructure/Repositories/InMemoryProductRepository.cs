using System.Collections.Concurrent;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;

namespace ECommerceInventory.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        return Task.FromResult(_products.Values.AsEnumerable());
    }

    public Task<Product?> GetByIdAsync(Guid id)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task AddAsync(Product product)
    {
        product.RowVersion = BitConverter.GetBytes(1L);
        if (!_products.TryAdd(product.Id, product))
        {
            throw new InvalidOperationException($"Product with ID {product.Id} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product)
    {
        if (!_products.TryGetValue(product.Id, out var existingProduct))
        {
            throw new InvalidOperationException($"Product with ID {product.Id} not found.");
        }

        if (!existingProduct.RowVersion.SequenceEqual(product.RowVersion))
        {
            throw new InvalidOperationException(
                $"Concurrency conflict: Product with ID {product.Id} has been modified.");
        }

        var currentVersion = BitConverter.ToInt64(product.RowVersion, 0);
        product.RowVersion = BitConverter.GetBytes(currentVersion + 1);
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        if (!_products.TryRemove(id, out _))
        {
            throw new InvalidOperationException($"Product with ID {id} not found.");
        }
        return Task.CompletedTask;
    }
}
