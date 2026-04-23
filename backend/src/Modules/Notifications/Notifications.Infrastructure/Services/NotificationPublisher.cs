using AutoMapper;
using EduPlatform.Shared.Application.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Hubs;

namespace Notifications.Infrastructure.Services;

public class NotificationPublisher : INotificationDispatcher
{
    private readonly INotificationsDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationPublisher> _logger;

    public NotificationPublisher(
        INotificationsDbContext context,
        IMapper mapper,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationPublisher> logger)
    {
        _context = context;
        _mapper = mapper;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        var entity = Build(request);
        _context.Notifications.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await PushSafeAsync(entity, cancellationToken);
    }

    public async Task PublishManyAsync(IReadOnlyCollection<NotificationRequest> requests, CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0) return;

        var entities = requests.Select(Build).ToList();
        _context.Notifications.AddRange(entities);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var entity in entities)
            await PushSafeAsync(entity, cancellationToken);
    }

    private static Notification Build(NotificationRequest request) => new()
    {
        UserId = request.UserId,
        Type = request.Type,
        Title = request.Title,
        Message = request.Message,
        LinkUrl = request.LinkUrl,
        IsRead = false,
        CreatedAt = DateTime.UtcNow
    };

    private async Task PushSafeAsync(Notification entity, CancellationToken cancellationToken)
    {
        try
        {
            var dto = _mapper.Map<NotificationDto>(entity);
            await _hubContext.Clients.Group(entity.UserId).SendAsync("ReceiveNotification", dto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push SignalR notification for user {UserId}", entity.UserId);
        }
    }
}
