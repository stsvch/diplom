namespace Messaging.Application.Interfaces;

public interface IChatConnectionTracker
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string userId, string connectionId);
    IReadOnlyCollection<string> GetConnections(string userId);
}
