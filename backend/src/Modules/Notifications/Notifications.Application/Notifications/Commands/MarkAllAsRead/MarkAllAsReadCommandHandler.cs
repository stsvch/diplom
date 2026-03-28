using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Interfaces;

namespace Notifications.Application.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result>
{
    private readonly INotificationsDbContext _context;

    public MarkAllAsReadCommandHandler(INotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
            notification.IsRead = true;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
