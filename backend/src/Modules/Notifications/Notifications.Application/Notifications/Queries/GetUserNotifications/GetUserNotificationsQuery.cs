// Файл: GetUserNotificationsQuery.cs
using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Notifications.Application.DTOs;

namespace Notifications.Application.Notifications.Queries.GetUserNotifications;

// Запрос GetUserNotificationsQuery описывает параметры чтения для MediatR-обработчика.
public record GetUserNotificationsQuery(
    string UserId,
    NotificationType? Type,
    bool? IsRead,
    int Page,
    int PageSize
) : IRequest<Result<PagedResult<NotificationDto>>>;
