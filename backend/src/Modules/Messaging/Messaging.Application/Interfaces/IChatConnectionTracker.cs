// IChatConnectionTracker.cs
namespace Messaging.Application.Interfaces;

// Контракт application-слоя отделяет бизнес-сценарии от конкретной инфраструктуры.
public interface IChatConnectionTracker
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string userId, string connectionId);
    IReadOnlyCollection<string> GetConnections(string userId);
}
