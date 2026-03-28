using AutoMapper;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;

namespace Notifications.Application.Notifications.Queries.GetUserNotifications;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, Result<PagedResult<NotificationDto>>>
{
    private readonly INotificationsDbContext _context;
    private readonly IMapper _mapper;

    public GetUserNotificationsQueryHandler(INotificationsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications.Where(n => n.UserId == request.UserId);

        if (request.Type.HasValue)
            query = query.Where(n => n.Type == request.Type.Value);

        if (request.IsRead.HasValue)
            query = query.Where(n => n.IsRead == request.IsRead.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<NotificationDto>>(items);

        return Result.Success(new PagedResult<NotificationDto>(dtos, totalCount, request.Page, request.PageSize));
    }
}
