// Файл: INotificationSender.cs
using Notifications.Application.DTOs;

namespace Notifications.Application.Interfaces;

// Интерфейс INotificationSender задаёт контракт сервиса между слоями или модулями.
public interface INotificationSender
{
    Task SendAsync(string userId, NotificationDto notification);
}
