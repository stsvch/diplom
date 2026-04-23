using Calendar.Application.Interfaces;
using Calendar.Domain.Entities;
using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Services;

public class CalendarEventPublisher : ICalendarEventPublisher
{
    private readonly ICalendarDbContext _context;

    public CalendarEventPublisher(ICalendarDbContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(CalendarEventUpsert request, CancellationToken cancellationToken = default)
    {
        var existing = await FindSingleAsync(request.SourceType, request.SourceId, request.UserId, cancellationToken);
        if (existing is null)
            _context.CalendarEvents.Add(BuildNew(request));
        else
            ApplyUpdate(existing, request);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertManyAsync(IReadOnlyCollection<CalendarEventUpsert> requests, CancellationToken cancellationToken = default)
    {
        if (requests.Count == 0) return;

        var sourceTypes = requests.Select(r => r.SourceType).Distinct().ToList();
        var sourceIds = requests.Select(r => r.SourceId).Distinct().ToList();

        // EF can't Contains over composite tuples — pull by IN clauses then filter client-side
        var candidates = await _context.CalendarEvents
            .Where(e => sourceTypes.Contains(e.SourceType!) && sourceIds.Contains(e.SourceId!.Value))
            .ToListAsync(cancellationToken);

        var map = candidates.ToLookup(c => (c.SourceType, c.SourceId, c.UserId));

        foreach (var request in requests)
        {
            var existing = map[(request.SourceType, request.SourceId, request.UserId)].FirstOrDefault();
            if (existing is null)
                _context.CalendarEvents.Add(BuildNew(request));
            else
                ApplyUpdate(existing, request);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBySourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default)
    {
        await _context.CalendarEvents
            .Where(e => e.SourceType == sourceType && e.SourceId == sourceId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteBySourceForUserAsync(string sourceType, Guid sourceId, string userId, CancellationToken cancellationToken = default)
    {
        await _context.CalendarEvents
            .Where(e => e.SourceType == sourceType && e.SourceId == sourceId && e.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteByCourseForUserAsync(Guid courseId, string userId, CancellationToken cancellationToken = default)
    {
        await _context.CalendarEvents
            .Where(e => e.CourseId == courseId && e.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteByCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        await _context.CalendarEvents
            .Where(e => e.CourseId == courseId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private Task<CalendarEvent?> FindSingleAsync(string sourceType, Guid sourceId, string? userId, CancellationToken cancellationToken)
    {
        return _context.CalendarEvents
            .FirstOrDefaultAsync(
                e => e.SourceType == sourceType && e.SourceId == sourceId && e.UserId == userId,
                cancellationToken);
    }

    private static CalendarEvent BuildNew(CalendarEventUpsert request) => new()
    {
        UserId = request.UserId,
        CourseId = request.CourseId,
        Title = request.Title,
        Description = request.Description,
        EventDate = DateTime.SpecifyKind(request.EventDate, DateTimeKind.Utc),
        EventTime = request.EventTime,
        Type = request.Type,
        SourceType = request.SourceType,
        SourceId = request.SourceId,
        CreatedAt = DateTime.UtcNow
    };

    private static void ApplyUpdate(CalendarEvent entity, CalendarEventUpsert request)
    {
        entity.CourseId = request.CourseId;
        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.EventDate = DateTime.SpecifyKind(request.EventDate, DateTimeKind.Utc);
        entity.EventTime = request.EventTime;
        entity.Type = request.Type;
    }
}
