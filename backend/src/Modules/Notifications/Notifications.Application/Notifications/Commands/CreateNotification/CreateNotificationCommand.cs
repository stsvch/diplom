using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Notifications.Application.DTOs;

namespace Notifications.Application.Notifications.Commands.CreateNotification;

public record CreateNotificationCommand(
    string UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? LinkUrl
) : IRequest<Result<NotificationDto>>;
