using System.Collections.Concurrent;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;

namespace ECommerceInventory.Infrastructure.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public Task<User?> GetByIdAsync(Guid id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.Values.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task AddAsync(User user)
    {
        if (!_users.TryAdd(user.Id, user))
        {
            throw new InvalidOperationException($"User with ID {user.Id} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user)
    {
        if (!_users.ContainsKey(user.Id))
        {
            throw new InvalidOperationException($"User with ID {user.Id} not found.");
        }
        _users[user.Id] = user;
        return Task.CompletedTask;
    }
}
