using System.Collections.Concurrent;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;

namespace ECommerceInventory.Infrastructure.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task<Order?> GetByIdAsync(Guid id)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
    {
        var orders = _orders.Values.Where(o => o.UserId == userId);
        return Task.FromResult(orders);
    }

    public Task AddAsync(Order order)
    {
        if (!_orders.TryAdd(order.Id, order))
        {
            throw new InvalidOperationException($"Order with ID {order.Id} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order)
    {
        if (!_orders.ContainsKey(order.Id))
        {
            throw new InvalidOperationException($"Order with ID {order.Id} not found.");
        }
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
}
