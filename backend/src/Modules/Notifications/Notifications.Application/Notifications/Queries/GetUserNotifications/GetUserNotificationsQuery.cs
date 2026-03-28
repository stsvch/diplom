using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain;
using MediatR;
using Notifications.Application.DTOs;
using Notifications.Domain.Enums;

namespace Notifications.Application.Notifications.Queries.GetUserNotifications;

public record GetUserNotificationsQuery(
    string UserId,
    NotificationType? Type,
    bool? IsRead,
    int Page,
    int PageSize
) : IRequest<Result<PagedResult<NotificationDto>>>;
