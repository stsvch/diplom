using EduPlatform.Shared.Domain;
using MediatR;
using Notifications.Application.DTOs;
using Notifications.Domain.Enums;

namespace Notifications.Application.Notifications.Commands.CreateNotification;

public record CreateNotificationCommand(
    string UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? LinkUrl
) : IRequest<Result<NotificationDto>>;
