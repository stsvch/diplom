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
    private readonly ISubscriptionEntitlementProvider _entitlements;
    private readonly IEnrollmentReadService _enrollment;
    private readonly INotificationDispatcher _notifications;
    private readonly ICalendarEventPublisher _calendar;

    public BookSlotCommandHandler(
        ISchedulingDbContext context,
        ISubscriptionEntitlementProvider entitlements,
        IEnrollmentReadService enrollment,
        INotificationDispatcher notifications,
        ICalendarEventPublisher calendar)
    {
        _context = context;
        _entitlements = entitlements;
        _enrollment = enrollment;
        _notifications = notifications;
        _calendar = calendar;
    }

    public async Task<Result<string>> Handle(BookSlotCommand request, CancellationToken cancellationToken)
    {
        var availability = await _context.TeacherAvailabilities
            .FirstOrDefaultAsync(a => a.Id == request.TeacherAvailabilityId, cancellationToken);

        if (availability == null || !availability.IsActive)
            return Result.Failure<string>("Расписание недоступно.");

        var startTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
        if (startTime <= DateTime.UtcNow)
            return Result.Failure<string>("Нельзя записаться на прошедшее время.");

        if (!IsTimeAllowedByRule(availability, startTime))
            return Result.Failure<string>("Это время не входит в расписание преподавателя.");

        var endTime = startTime.AddMinutes(availability.SlotDurationMinutes);

        if (availability.RequiredCourseId.HasValue)
        {
            var enrolledCourses = await _enrollment.GetActiveCourseIdsForStudentAsync(request.StudentId, cancellationToken);
            if (!enrolledCourses.Contains(availability.RequiredCourseId.Value))
                return Result.Failure<string>("Это занятие доступно только студентам соответствующего курса.");
        }

        var entitlements = await _entitlements.GetForUserAsync(request.StudentId, cancellationToken);
        var requiredKind = availability.SessionType == SessionType.Individual
            ? LiveSlotKind.Individual
            : LiveSlotKind.Group;

        if (!entitlements.HasActiveSubscription)
            return Result.Failure<string>("Бронирование живых занятий доступно только подписчикам.");

        if (!entitlements.CanBook(requiredKind))
        {
            return requiredKind == LiveSlotKind.Individual
                ? Result.Failure<string>("Лимит индивидуальных занятий за месяц исчерпан.")
                : Result.Failure<string>("Лимит групповых занятий за месяц исчерпан.");
        }

        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(
                s => s.AvailabilityId == availability.Id && s.StartTime == startTime,
                cancellationToken);

        if (slot == null)
        {
            slot = new ScheduleSlot
            {
                TeacherId = availability.TeacherId,
                TeacherName = availability.TeacherName,
                AvailabilityId = availability.Id,
                Title = availability.Title,
                Description = availability.Description,
                StartTime = startTime,
                EndTime = endTime,
                SessionType = availability.SessionType,
                MaxStudents = availability.MaxStudents,
                RequiredCourseId = availability.RequiredCourseId,
                MeetingLink = availability.MeetingLink,
                Status = SlotStatus.Available
            };
            _context.ScheduleSlots.Add(slot);
        }
        else
        {
            if (slot.Status == SlotStatus.Cancelled || slot.Status == SlotStatus.Completed)
                return Result.Failure<string>("Слот недоступен для записи.");

            var activeBookings = slot.Bookings.Where(b => b.Status == BookingStatus.Booked).ToList();
            if (activeBookings.Any(b => b.StudentId == request.StudentId))
                return Result.Failure<string>("Вы уже записаны на это занятие.");
            if (activeBookings.Count >= slot.MaxStudents)
                return Result.Failure<string>("Нет свободных мест.");
        }

        var booking = new SessionBooking
        {
            SlotId = slot.Id,
            StudentId = request.StudentId,
            StudentName = request.StudentName,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            BookedAt = DateTime.UtcNow,
            Status = BookingStatus.Booked
        };
        _context.SessionBookings.Add(booking);

        var totalActive = slot.Bookings.Count(b => b.Status == BookingStatus.Booked) + 1;
        if (totalActive >= slot.MaxStudents)
            slot.Status = SlotStatus.Full;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsExclusionViolation(ex))
        {
            return Result.Failure<string>("Это время уже занято — попробуйте другой слот.");
        }

        await _entitlements.RecordUsageAsync(
            new SlotUsageRecord(request.StudentId, requiredKind, booking.Id),
            cancellationToken);

        await _calendar.UpsertAsync(new CalendarEventUpsert(
            request.StudentId, slot.RequiredCourseId, slot.Title, slot.Description,
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

    private static bool IsTimeAllowedByRule(Domain.Entities.TeacherAvailability rule, DateTime startUtc)
    {
        var localDate = DateOnly.FromDateTime(startUtc);
        var localTime = TimeOnly.FromDateTime(startUtc);

        if (localDate < rule.ValidFrom) return false;
        if (rule.ValidUntil.HasValue && localDate > rule.ValidUntil.Value) return false;

        if (rule.Kind == AvailabilityKind.OneOff)
        {
            if (rule.SpecificDate != localDate) return false;
        }
        else
        {
            if (!rule.DayOfWeek.HasValue || startUtc.DayOfWeek != rule.DayOfWeek.Value) return false;
        }

        if (localTime < rule.StartTime || localTime >= rule.EndTime) return false;

        var minutesFromRuleStart = (int)(localTime - rule.StartTime).TotalMinutes;
        var step = rule.SlotDurationMinutes + rule.BreakBetweenMinutes;
        if (step <= 0) return false;

        return minutesFromRuleStart % step == 0;
    }

    private static bool IsExclusionViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("CK_ScheduleSlots_NoTeacherOverlap", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("CK_SessionBookings_NoStudentOverlap", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("23P01", StringComparison.OrdinalIgnoreCase) == true;
    }
}
