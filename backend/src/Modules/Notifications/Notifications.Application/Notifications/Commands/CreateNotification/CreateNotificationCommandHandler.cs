using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;

namespace Notifications.Application.Notifications.Commands.CreateNotification;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<NotificationDto>>
{
    private readonly INotificationsDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationSender _sender;

    public CreateNotificationCommandHandler(INotificationsDbContext context, IMapper mapper, INotificationSender sender)
    {
        _context = context;
        _mapper = mapper;
        _sender = sender;
    }

    public async Task<Result<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            LinkUrl = request.LinkUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<NotificationDto>(notification);

        await _sender.SendAsync(request.UserId, dto);

        return Result.Success(dto);
    }
}
