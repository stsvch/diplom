using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.BookSlot;

public class BookSlotCommandHandler : IRequestHandler<BookSlotCommand, Result<string>>
{
    private readonly ISchedulingDbContext _context;
    private readonly INotificationDispatcher _notifications;
    private readonly ICalendarEventPublisher _calendar;

    public BookSlotCommandHandler(
        ISchedulingDbContext context,
        INotificationDispatcher notifications,
        ICalendarEventPublisher calendar)
    {
        _context = context;
        _notifications = notifications;
        _calendar = calendar;
    }

    public async Task<Result<string>> Handle(BookSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken);

        if (slot == null)
            return Result.Failure<string>("Слот не найден.");

        if (slot.Status != SlotStatus.Available)
            return Result.Failure<string>("Слот недоступен для записи.");

        if (slot.StartTime <= DateTime.UtcNow)
            return Result.Failure<string>("Нельзя записаться на прошедшее занятие.");

        var activeBookings = slot.Bookings.Where(b => b.Status == BookingStatus.Booked).ToList();

        if (activeBookings.Any(b => b.StudentId == request.StudentId))
            return Result.Failure<string>("Вы уже записаны на это занятие.");

        if (!slot.IsGroupSession && activeBookings.Count >= slot.MaxStudents)
            return Result.Failure<string>("Слот уже занят.");

        if (slot.IsGroupSession && activeBookings.Count >= slot.MaxStudents)
            return Result.Failure<string>("Нет свободных мест.");

        var booking = new SessionBooking
        {
            SlotId = slot.Id,
            StudentId = request.StudentId,
            StudentName = request.StudentName,
            BookedAt = DateTime.UtcNow,
            Status = BookingStatus.Booked
        };

        _context.SessionBookings.Add(booking);

        // For non-group sessions, mark slot as booked once filled
        if (!slot.IsGroupSession)
        {
            slot.Status = SlotStatus.Booked;
        }
        else if (activeBookings.Count + 1 >= slot.MaxStudents)
        {
            slot.Status = SlotStatus.Booked;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _calendar.UpsertAsync(new CalendarEventUpsert(
            request.StudentId, slot.CourseId, slot.Title, slot.Description,
            DateTime.SpecifyKind(slot.StartTime.Date, DateTimeKind.Utc),
            slot.StartTime.ToString("HH:mm"),
            CalendarEventType.Workshop, "ScheduleSlot", slot.Id), cancellationToken);

        await _notifications.PublishAsync(new NotificationRequest(
            slot.TeacherId, NotificationType.Course,
            "Запись на занятие",
            $"{request.StudentName} записался на «{slot.Title}»",
            "/teacher/schedule"), cancellationToken);

        return Result.Success("Вы успешно записались на занятие.");
    }
}
