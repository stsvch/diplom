using Messaging.Application.Interfaces;
using System.Collections.Concurrent;

namespace Messaging.Infrastructure.Services;

public class ChatConnectionTracker : IChatConnectionTracker
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connections = new();

    public void AddConnection(string userId, string connectionId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(connectionId))
            return;

        var userConnections = _connections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
        userConnections[connectionId] = 0;
    }

    public void RemoveConnection(string userId, string connectionId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(connectionId))
            return;

        if (!_connections.TryGetValue(userId, out var userConnections))
            return;

        userConnections.TryRemove(connectionId, out _);
        if (userConnections.IsEmpty)
            _connections.TryRemove(userId, out _);
    }

    public IReadOnlyCollection<string> GetConnections(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Array.Empty<string>();

        return _connections.TryGetValue(userId, out var userConnections)
            ? userConnections.Keys.ToList()
            : Array.Empty<string>();
    }
}
