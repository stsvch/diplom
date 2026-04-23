using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result<string>>
{
    private readonly ISchedulingDbContext _context;
    private readonly ICalendarEventPublisher _calendar;
    private readonly INotificationDispatcher _notifications;

    public CancelBookingCommandHandler(
        ISchedulingDbContext context,
        ICalendarEventPublisher calendar,
        INotificationDispatcher notifications)
    {
        _context = context;
        _calendar = calendar;
        _notifications = notifications;
    }

    public async Task<Result<string>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken);

        if (slot == null)
            return Result.Failure<string>("Слот не найден.");

        var booking = slot.Bookings.FirstOrDefault(b => b.StudentId == request.StudentId && b.Status == BookingStatus.Booked);

        if (booking == null)
            return Result.Failure<string>("Запись не найдена.");

        booking.Status = BookingStatus.Cancelled;

        // Restore slot to Available if it was Booked
        if (slot.Status == SlotStatus.Booked)
        {
            slot.Status = SlotStatus.Available;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _calendar.DeleteBySourceForUserAsync("ScheduleSlot", slot.Id, request.StudentId, cancellationToken);

        await _notifications.PublishAsync(new NotificationRequest(
            slot.TeacherId, NotificationType.Message,
            "Запись отменена",
            $"Студент отменил запись на «{slot.Title}»",
            "/teacher/schedule"), cancellationToken);

        return Result.Success("Запись отменена.");
    }
}
