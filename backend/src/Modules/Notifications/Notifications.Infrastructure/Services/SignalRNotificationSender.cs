using Microsoft.AspNetCore.SignalR;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Infrastructure.Hubs;

namespace Notifications.Infrastructure.Services;

public class SignalRNotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationSender(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendAsync(string userId, NotificationDto notification)
    {
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
    }
}
