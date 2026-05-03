using Courses.Domain.Entities;
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Host.Services;

public class CourseItemSyncService
{
    private readonly CoursesDbContext _coursesDb;

    public CourseItemSyncService(CoursesDbContext coursesDb)
    {
        _coursesDb = coursesDb;
    }

    public async Task EnsureLessonItemAsync(Guid lessonId, CancellationToken cancellationToken)
    {
        var lesson = await _coursesDb.Lessons
            .AsNoTracking()
            .Where(l => l.Id == lessonId)
            .Select(l => new
            {
                l.Id,
                l.ModuleId,
                CourseId = l.Module.CourseId,
                l.Title,
                l.Description,
                l.OrderIndex,
                l.IsPublished
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            return;

        await EnsureItemAsync(
            lesson.CourseId,
            lesson.ModuleId,
            CourseItemType.Lesson,
            lesson.Id,
            lesson.Title,
            lesson.Description,
            lesson.OrderIndex,
            lesson.IsPublished ? CourseItemStatus.Published : CourseItemStatus.Draft,
            points: null,
            deadline: null,
            cancellationToken);
    }

    public Task EnsureTestItemAsync(
        Guid courseId,
        Guid testId,
        string title,
        string? description,
        int maxScore,
        DateTime? deadline,
        CancellationToken cancellationToken)
    {
        return EnsureItemAsync(
            courseId,
            moduleId: null,
            CourseItemType.Test,
            testId,
            title,
            description,
            orderIndex: null,
            CourseItemStatus.NeedsContent,
            maxScore > 0 ? maxScore : null,
            deadline,
            cancellationToken);
    }

    public Task EnsureAssignmentItemAsync(
        Guid courseId,
        Guid assignmentId,
        string title,
        string? description,
        int maxScore,
        DateTime? deadline,
        CancellationToken cancellationToken)
    {
        var status = !string.IsNullOrWhiteSpace(description) && maxScore > 0
            ? CourseItemStatus.Ready
            : CourseItemStatus.NeedsContent;

        return EnsureItemAsync(
            courseId,
            moduleId: null,
            CourseItemType.Assignment,
            assignmentId,
            title,
            description,
            orderIndex: null,
            status,
            maxScore > 0 ? maxScore : null,
            deadline,
            cancellationToken);
    }

    public Task EnsureLiveSessionItemAsync(
        Guid? courseId,
        Guid slotId,
        string title,
        string? description,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken)
    {
        if (!courseId.HasValue)
            return Task.CompletedTask;

        return EnsureItemAsync(
            courseId.Value,
            moduleId: null,
            CourseItemType.LiveSession,
            slotId,
            title,
            description,
            orderIndex: null,
            startTime < endTime ? CourseItemStatus.Ready : CourseItemStatus.NeedsContent,
            points: null,
            deadline: startTime,
            cancellationToken);
    }

    public async Task DeleteBySourceAsync(
        CourseItemType type,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var item = await _coursesDb.CourseItems
            .FirstOrDefaultAsync(i => i.Type == type && i.SourceId == sourceId, cancellationToken);

        if (item is null)
            return;

        _coursesDb.CourseItems.Remove(item);
        await _coursesDb.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureItemAsync(
        Guid courseId,
        Guid? moduleId,
        CourseItemType type,
        Guid sourceId,
        string title,
        string? description,
        int? orderIndex,
        CourseItemStatus status,
        decimal? points,
        DateTime? deadline,
        CancellationToken cancellationToken)
    {
        var item = await _coursesDb.CourseItems
            .FirstOrDefaultAsync(i => i.Type == type && i.SourceId == sourceId, cancellationToken);

        var isNew = item is null;
        if (item is null)
        {
            item = new CourseItem
            {
                CourseId = courseId,
                ModuleId = moduleId,
                Type = type,
                SourceId = sourceId,
                OrderIndex = orderIndex ?? await GetNextOrderIndexAsync(courseId, moduleId, cancellationToken)
            };

            _coursesDb.CourseItems.Add(item);
        }

        if (!isNew && item.CourseId != courseId && !moduleId.HasValue)
            item.ModuleId = null;

        item.CourseId = courseId;
        if (isNew || moduleId.HasValue)
            item.ModuleId = moduleId;

        item.Title = string.IsNullOrWhiteSpace(title) ? type.ToString() : title;
        item.Description = description;
        item.Status = status;
        item.Points = points;
        item.Deadline = deadline;

        await _coursesDb.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> GetNextOrderIndexAsync(
        Guid courseId,
        Guid? moduleId,
        CancellationToken cancellationToken)
    {
        return await _coursesDb.CourseItems
            .Where(i => i.CourseId == courseId && i.ModuleId == moduleId)
            .MaxAsync(i => (int?)i.OrderIndex, cancellationToken) + 1 ?? 0;
    }
}
