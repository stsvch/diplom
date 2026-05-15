// Файл: CreateNotificationCommand.cs
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Notifications.Application.DTOs;

namespace Notifications.Application.Notifications.Commands.CreateNotification;

// Команда CreateNotificationCommand переносит входные данные операции изменения состояния в MediatR.
public record CreateNotificationCommand(
    string UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? LinkUrl
) : IRequest<Result<NotificationDto>>;
