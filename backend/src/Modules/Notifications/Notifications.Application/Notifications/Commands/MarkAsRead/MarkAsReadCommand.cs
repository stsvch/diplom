// Файл: MarkAsReadCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Notifications.Application.Notifications.Commands.MarkAsRead;

// Команда MarkAsReadCommand переносит входные данные операции изменения состояния в MediatR.
public record MarkAsReadCommand(Guid Id, string UserId) : IRequest<Result>;
