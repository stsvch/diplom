using EduPlatform.Shared.Domain.Enums;

namespace EduPlatform.Shared.Application.Contracts;

public record NotificationRequest(
    string UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? LinkUrl);

public interface INotificationDispatcher
{
    Task PublishAsync(NotificationRequest request, CancellationToken cancellationToken = default);

    Task PublishManyAsync(IReadOnlyCollection<NotificationRequest> requests, CancellationToken cancellationToken = default);
}
