using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Interfaces;

namespace Notifications.Application.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    private readonly INotificationsDbContext _context;

    public GetUnreadCountQueryHandler(INotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _context.Notifications
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);

        return Result.Success(count);
    }
}
