using System.Collections.Concurrent;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;

namespace ECommerceInventory.Infrastructure.Repositories;

public class InMemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<Guid, Session> _sessions = new();

    public Task<Session?> GetByIdAsync(Guid id)
    {
        _sessions.TryGetValue(id, out var session);
        return Task.FromResult(session);
    }

    public Task<Session?> GetByTokenAsync(string token)
    {
        var session = _sessions.Values.FirstOrDefault(s => s.Token == token);
        return Task.FromResult(session);
    }

    public Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId)
    {
        var sessions = _sessions.Values.Where(s => s.UserId == userId);
        return Task.FromResult(sessions);
    }

    public Task<IEnumerable<Session>> GetActiveByUserIdAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var sessions = _sessions.Values
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > now);
        return Task.FromResult(sessions);
    }

    public Task AddAsync(Session session)
    {
        if (!_sessions.TryAdd(session.Id, session))
        {
            throw new InvalidOperationException($"Session with ID {session.Id} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Session session)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task RevokeAllByUserIdAsync(Guid userId)
    {
        var userSessions = _sessions.Values.Where(s => s.UserId == userId).ToList();
        foreach (var session in userSessions)
        {
            session.IsRevoked = true;
        }
        return Task.CompletedTask;
    }
}
