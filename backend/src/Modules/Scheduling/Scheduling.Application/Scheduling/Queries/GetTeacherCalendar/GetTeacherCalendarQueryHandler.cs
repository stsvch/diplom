// GetTeacherCalendarQueryHandler.cs

using EduPlatform.Shared.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Queries.GetTeacherCalendar;

/// <summary>
/// Обработчик CQRS-запроса GetTeacherCalendarQuery: читает данные, применяет фильтры и возвращает DTO/Result.
/// </summary>
public class GetTeacherCalendarQueryHandler : IRequestHandler<GetTeacherCalendarQuery, List<CalendarSlotDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IEnrollmentReadService _enrollment;

    public GetTeacherCalendarQueryHandler(
        ISchedulingDbContext context,
        IEnrollmentReadService enrollment)
    {
        _context = context;
        _enrollment = enrollment;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<List<CalendarSlotDto>> Handle(GetTeacherCalendarQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.From.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.To.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var rules = await _context.TeacherAvailabilities
            .Where(a => a.TeacherId == request.TeacherId
                     && a.IsActive
                     && a.ValidFrom <= request.To
                     && (a.ValidUntil == null || a.ValidUntil >= request.From))
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
            return new List<CalendarSlotDto>();

        // Скрываем правила, требующие курса, если студент не записан
        var allowedRules = rules;
        if (!string.IsNullOrEmpty(request.CurrentUserId))
        {
            var requiredCourseIds = rules
                .Where(r => r.RequiredCourseId.HasValue)
                .Select(r => r.RequiredCourseId!.Value)
                .Distinct()
                .ToList();

            if (requiredCourseIds.Count > 0)
            {
                var enrolled = (await _enrollment.GetActiveCourseIdsForStudentAsync(request.CurrentUserId, cancellationToken))
                    .ToHashSet();
                allowedRules = rules
                    .Where(r => !r.RequiredCourseId.HasValue || enrolled.Contains(r.RequiredCourseId.Value))
                    .ToList();
            }
        }

        var ruleIds = allowedRules.Select(r => r.Id).ToList();

        var materializedSlots = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .Where(s => s.AvailabilityId.HasValue
                     && ruleIds.Contains(s.AvailabilityId!.Value)
                     && s.StartTime >= fromUtc
                     && s.StartTime <= toUtc
                     && s.Status != SlotStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var materializedByKey = materializedSlots.ToDictionary(s => (s.AvailabilityId!.Value, s.StartTime));

        var result = new List<CalendarSlotDto>();
        var now = DateTime.UtcNow;

        foreach (var rule in allowedRules)
        {
            foreach (var (start, end) in EnumerateSlotsForRule(rule, request.From, request.To))
            {
                if (start <= now) continue;

                if (materializedByKey.TryGetValue((rule.Id, start), out var existing))
                {
                    var bookedCount = existing.Bookings.Count(b => b.Status == BookingStatus.Booked);
                    var bookedByMe = !string.IsNullOrEmpty(request.CurrentUserId)
                        && existing.Bookings.Any(b => b.StudentId == request.CurrentUserId && b.Status == BookingStatus.Booked);
                    result.Add(new CalendarSlotDto
                    {
                        AvailabilityId = rule.Id,
                        SlotId = existing.Id,
                        StartTime = existing.StartTime,
                        EndTime = existing.EndTime,
                        SessionType = existing.SessionType,
                        MaxStudents = existing.MaxStudents,
                        BookedCount = bookedCount,
                        Title = existing.Title,
                        Description = existing.Description,
                        RequiredCourseId = existing.RequiredCourseId,
                        IsBookedByCurrentUser = bookedByMe
                    });
                }
                else
                {
                    result.Add(new CalendarSlotDto
                    {
                        AvailabilityId = rule.Id,
                        SlotId = null,
                        StartTime = start,
                        EndTime = end,
                        SessionType = rule.SessionType,
                        MaxStudents = rule.MaxStudents,
                        BookedCount = 0,
                        Title = rule.Title,
                        Description = rule.Description,
                        RequiredCourseId = rule.RequiredCourseId,
                        IsBookedByCurrentUser = false
                    });
                }
            }
        }

        return result.OrderBy(s => s.StartTime).ToList();
    }

    private static IEnumerable<(DateTime Start, DateTime End)> EnumerateSlotsForRule(
        TeacherAvailability rule, DateOnly fromDate, DateOnly toDate)
    {
        var step = rule.SlotDurationMinutes + rule.BreakBetweenMinutes;
        if (step <= 0) yield break;

        var rangeStart = fromDate > rule.ValidFrom ? fromDate : rule.ValidFrom;
        var rangeEnd = rule.ValidUntil.HasValue && rule.ValidUntil.Value < toDate ? rule.ValidUntil.Value : toDate;

        for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            if (rule.Kind == AvailabilityKind.OneOff)
            {
                if (rule.SpecificDate != date) continue;
            }
            else
            {
                if (!rule.DayOfWeek.HasValue || rule.DayOfWeek.Value != date.DayOfWeek)
                    continue;
            }

            for (var time = rule.StartTime;
                 time.AddMinutes(rule.SlotDurationMinutes) <= rule.EndTime;
                 time = time.AddMinutes(step))
            {
                var startUtc = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Utc);
                var endUtc = startUtc.AddMinutes(rule.SlotDurationMinutes);
                yield return (startUtc, endUtc);
            }
        }
    }
}
