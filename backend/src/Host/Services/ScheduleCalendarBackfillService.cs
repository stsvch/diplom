// Файл: ScheduleCalendarBackfillService.cs
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace EduPlatform.Host.Services;

// Фоновый сервис ScheduleCalendarBackfillService выполняет периодическую задачу вне HTTP-запроса.
public class ScheduleCalendarBackfillService : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduleCalendarBackfillService> _logger;

    public ScheduleCalendarBackfillService(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduleCalendarBackfillService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var schedulingDb = scope.ServiceProvider.GetRequiredService<ISchedulingDbContext>();
            var calendar = scope.ServiceProvider.GetRequiredService<ICalendarEventPublisher>();

            var now = DateTime.UtcNow;

            var activeBookings = await schedulingDb.SessionBookings
                .Where(b => b.Status == BookingStatus.Booked && b.StartTime >= now)
                .Join(
                    schedulingDb.ScheduleSlots,
                    b => b.SlotId,
                    s => s.Id,
                    (b, s) => new
                    {
                        b.StudentId,
                        s.TeacherId,
                        s.Id,
                        s.Title,
                        s.Description,
                        s.RequiredCourseId,
                        s.StartTime
                    })
                .ToListAsync(stoppingToken);

            if (activeBookings.Count == 0) return;

            var upserts = new List<CalendarEventUpsert>(activeBookings.Count * 2);
            var teacherEventsAdded = new HashSet<Guid>();

            foreach (var b in activeBookings)
            {
                upserts.Add(new CalendarEventUpsert(
                    b.StudentId, b.RequiredCourseId, b.Title, b.Description,
                    DateTime.SpecifyKind(b.StartTime.Date, DateTimeKind.Utc),
                    b.StartTime.ToString("HH:mm"),
                    CalendarEventType.Workshop, "ScheduleSlot", b.Id));

                if (teacherEventsAdded.Add(b.Id))
                {
                    upserts.Add(new CalendarEventUpsert(
                        b.TeacherId, b.RequiredCourseId, b.Title, b.Description,
                        DateTime.SpecifyKind(b.StartTime.Date, DateTimeKind.Utc),
                        b.StartTime.ToString("HH:mm"),
                        CalendarEventType.Workshop, "ScheduleSlot", b.Id));
                }
            }

            await calendar.UpsertManyAsync(upserts, stoppingToken);

            _logger.LogInformation(
                "ScheduleCalendarBackfill: synchronized {Count} calendar events for {Slots} slots.",
                upserts.Count, teacherEventsAdded.Count);
        }
        catch (Exception ex) when (ex is not TaskCanceledException)
        {
            _logger.LogError(ex, "ScheduleCalendarBackfill failed.");
        }
    }
}
