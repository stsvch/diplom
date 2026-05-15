// Файл: INotificationDispatcher.cs
using EduPlatform.Shared.Domain.Enums;

namespace EduPlatform.Shared.Application.Contracts;

// Межмодульный контракт NotificationRequest передаёт минимальные данные между контекстами модулей.
public record NotificationRequest(
    string UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? LinkUrl);

// Интерфейс INotificationDispatcher задаёт контракт сервиса между слоями или модулями.
public interface INotificationDispatcher
{
    Task PublishAsync(NotificationRequest request, CancellationToken cancellationToken = default);

    Task PublishManyAsync(IReadOnlyCollection<NotificationRequest> requests, CancellationToken cancellationToken = default);
}
