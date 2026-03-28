using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Interfaces;

namespace Notifications.Application.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly INotificationsDbContext _context;

    public MarkAsReadCommandHandler(INotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == request.UserId, cancellationToken);

        if (notification is null)
            return Result.Failure("Уведомление не найдено.");

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
