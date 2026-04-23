using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.CancelSlot;

public class CancelSlotCommandHandler : IRequestHandler<CancelSlotCommand, Result<string>>
{
    private readonly ISchedulingDbContext _context;
    private readonly ICalendarEventPublisher _calendar;
    private readonly INotificationDispatcher _notifications;

    public CancelSlotCommandHandler(
        ISchedulingDbContext context,
        ICalendarEventPublisher calendar,
        INotificationDispatcher notifications)
    {
        _context = context;
        _calendar = calendar;
        _notifications = notifications;
    }

    public async Task<Result<string>> Handle(CancelSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slot == null)
            return Result.Failure<string>("Слот не найден.");

        if (slot.TeacherId != request.TeacherId)
            return Result.Failure<string>("Нет доступа к этому слоту.");

        if (slot.Status == SlotStatus.Cancelled)
            return Result.Failure<string>("Слот уже отменён.");

        slot.Status = SlotStatus.Cancelled;

        var affectedStudents = new List<string>();
        foreach (var booking in slot.Bookings.Where(b => b.Status == BookingStatus.Booked))
        {
            booking.Status = BookingStatus.Cancelled;
            affectedStudents.Add(booking.StudentId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _calendar.DeleteBySourceAsync("ScheduleSlot", slot.Id, cancellationToken);

        if (affectedStudents.Count > 0)
        {
            var notifications = affectedStudents.Select(sid => new NotificationRequest(
                sid, NotificationType.Message,
                "Занятие отменено",
                $"«{slot.Title}» на {slot.StartTime:dd.MM.yyyy HH:mm}",
                "/student/schedule")).ToList();
            await _notifications.PublishManyAsync(notifications, cancellationToken);
        }

        return Result.Success("Слот отменён.");
    }
}
