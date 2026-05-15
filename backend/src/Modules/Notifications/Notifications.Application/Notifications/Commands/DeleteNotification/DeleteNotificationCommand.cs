// Файл: DeleteNotificationCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Notifications.Application.Notifications.Commands.DeleteNotification;

// Команда DeleteNotificationCommand переносит входные данные операции изменения состояния в MediatR.
public record DeleteNotificationCommand(Guid Id, string UserId) : IRequest<Result>;
