using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Interfaces;

namespace Notifications.Application.Notifications.Commands.DeleteNotification;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly INotificationsDbContext _context;

    public DeleteNotificationCommandHandler(INotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == request.UserId, cancellationToken);

        if (notification is null)
            return Result.Failure("Уведомление не найдено.");

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
