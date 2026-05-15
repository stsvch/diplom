// Файл: MarkAllAsReadCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Notifications.Application.Notifications.Commands.MarkAllAsRead;

// Команда MarkAllAsReadCommand переносит входные данные операции изменения состояния в MediatR.
public record MarkAllAsReadCommand(string UserId) : IRequest<Result>;
