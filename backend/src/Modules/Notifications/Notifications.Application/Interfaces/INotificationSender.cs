using Notifications.Application.DTOs;

namespace Notifications.Application.Interfaces;

public interface INotificationSender
{
    Task SendAsync(string userId, NotificationDto notification);
}
