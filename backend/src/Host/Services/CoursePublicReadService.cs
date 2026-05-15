// Файл: CoursePublicReadService.cs
using Assignments.Infrastructure.Persistence;
using Content.Infrastructure.Persistence;
using Courses.Application.DTOs;
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Tests.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

// Сервис чтения CoursePublicReadService собирает модель чтения для API без изменения состояния.
public class CoursePublicReadService
{
    private readonly CoursesDbContext _coursesDb;
    private readonly ContentDbContext _contentDb;
    private readonly TestsDbContext _testsDb;
    private readonly AssignmentsDbContext _assignmentsDb;

    public CoursePublicReadService(
        CoursesDbContext coursesDb,
        ContentDbContext contentDb,
        TestsDbContext testsDb,
        AssignmentsDbContext assignmentsDb)
    {
        _coursesDb = coursesDb;
        _contentDb = contentDb;
        _testsDb = testsDb;
        _assignmentsDb = assignmentsDb;
    }

    public async Task<List<CourseDetailSectionDto>> GetSectionsAsync(
        Guid courseId,
        bool includeUnpublished,
        CancellationToken cancellationToken = default)
    {
        var sections = await _coursesDb.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.OrderIndex)
            .Select(m => new CourseDetailSectionDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                OrderIndex = m.OrderIndex,
                IsPublished = m.IsPublished
            })
            .ToListAsync(cancellationToken);

        // Проецируем строковые значения, чтобы обойти материализацию enum при старых данных в БД.
        // В базе могут оставаться legacy-значения, отсутствующие в текущем enum.
        var rawItems = await _coursesDb.CourseItems
            .AsNoTracking()
            .Where(i => i.CourseId == courseId)
            .OrderBy(i => i.OrderIndex)
            .Select(i => new CourseItemRow(
                i.Id,
                i.ModuleId,
                i.Type.ToString(),
                i.SourceId,
                i.Title,
                i.Description,
                i.Url,
                i.ResourceKind,
                i.OrderIndex,
                i.Status.ToString(),
                i.IsRequired,
                i.Points,
                i.AvailableFrom,
                i.Deadline))
            .ToListAsync(cancellationToken);

        var items = rawItems
            .Select(r => new { Row = r, Type = ParseType(r.TypeRaw) })
            .Where(x => x.Type.HasValue)
            .ToList();

        if (!includeUnpublished)
        {
            items = items
                .Where(x => x.Row.StatusRaw is "Ready" or "Published")
                .ToList();
        }

        if (items.Count == 0)
            return sections;

        var lessonIds = items.Where(x => x.Type == CourseItemType.Lesson).Select(x => x.Row.SourceId).ToList();
        var lessonInfos = lessonIds.Count == 0
            ? new Dictionary<Guid, LessonInfo>()
            : await LoadLessonInfosAsync(lessonIds, cancellationToken);

        var testIds = items.Where(x => x.Type == CourseItemType.Test).Select(x => x.Row.SourceId).ToList();
        var testInfos = testIds.Count == 0
            ? new Dictionary<Guid, TestInfo>()
            : await LoadTestInfosAsync(testIds, cancellationToken);

        var assignmentIds = items.Where(x => x.Type == CourseItemType.Assignment).Select(x => x.Row.SourceId).ToList();
        var assignmentInfos = assignmentIds.Count == 0
            ? new Dictionary<Guid, AssignmentInfo>()
            : await LoadAssignmentInfosAsync(assignmentIds, cancellationToken);

        var dtos = items
            .Select(x => MapItem(x.Row, x.Type!.Value, lessonInfos, testInfos, assignmentInfos))
            .ToList();

        var bySection = dtos
            .Where(d => d.sectionId.HasValue)
            .GroupBy(d => d.sectionId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.dto).OrderBy(x => x.OrderIndex).ToList());

        foreach (var section in sections)
        {
            if (bySection.TryGetValue(section.Id, out var sectionItems))
                section.Items = sectionItems;
        }

        var unsectioned = dtos
            .Where(d => !d.sectionId.HasValue)
            .Select(d => d.dto)
            .OrderBy(d => d.OrderIndex)
            .ToList();

        if (unsectioned.Count > 0)
        {
            sections.Add(new CourseDetailSectionDto
            {
                Id = Guid.Empty,
                Title = "Без раздела",
                OrderIndex = int.MaxValue,
                IsPublished = true,
                Items = unsectioned
            });
        }

        return sections;
    }

    private static CourseItemType? ParseType(string raw)
    {
        return Enum.TryParse<CourseItemType>(raw, ignoreCase: false, out var t) ? t : null;
    }

    private async Task<Dictionary<Guid, LessonInfo>> LoadLessonInfosAsync(
        List<Guid> lessonIds,
        CancellationToken cancellationToken)
    {
        var lessons = await _coursesDb.Lessons
            .AsNoTracking()
            .Where(l => lessonIds.Contains(l.Id))
            .Select(l => new { l.Id, l.Duration })
            .ToListAsync(cancellationToken);

        var blockCounts = await _contentDb.LessonBlocks
            .AsNoTracking()
            .Where(b => lessonIds.Contains(b.LessonId))
            .GroupBy(b => b.LessonId)
            .Select(g => new { LessonId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.LessonId, x => x.Count, cancellationToken);

        return lessons.ToDictionary(
            l => l.Id,
            l => new LessonInfo(
                l.Duration,
                blockCounts.TryGetValue(l.Id, out var c) ? c : 0));
    }

    private async Task<Dictionary<Guid, TestInfo>> LoadTestInfosAsync(
        List<Guid> testIds,
        CancellationToken cancellationToken)
    {
        return await _testsDb.Tests
            .AsNoTracking()
            .Where(t => testIds.Contains(t.Id))
            .Select(t => new { t.Id, t.MaxScore, QuestionsCount = t.Questions.Count, t.Deadline })
            .ToDictionaryAsync(t => t.Id, t => new TestInfo(t.QuestionsCount, t.MaxScore, t.Deadline), cancellationToken);
    }

    private async Task<Dictionary<Guid, AssignmentInfo>> LoadAssignmentInfosAsync(
        List<Guid> assignmentIds,
        CancellationToken cancellationToken)
    {
        return await _assignmentsDb.Assignments
            .AsNoTracking()
            .Where(a => assignmentIds.Contains(a.Id))
            .Select(a => new { a.Id, a.MaxScore, a.Deadline })
            .ToDictionaryAsync(a => a.Id, a => new AssignmentInfo(a.MaxScore, a.Deadline), cancellationToken);
    }

    private static (Guid? sectionId, CourseDetailItemDto dto) MapItem(
        CourseItemRow row,
        CourseItemType type,
        Dictionary<Guid, LessonInfo> lessonInfos,
        Dictionary<Guid, TestInfo> testInfos,
        Dictionary<Guid, AssignmentInfo> assignmentInfos)
    {
        var dto = new CourseDetailItemDto
        {
            Id = row.Id,
            SourceId = row.SourceId,
            Type = type.ToString(),
            Title = row.Title,
            Description = row.Description,
            OrderIndex = row.OrderIndex,
            Status = row.StatusRaw,
            IsRequired = row.IsRequired,
            Points = row.Points,
            Deadline = row.Deadline,
            AvailableFrom = row.AvailableFrom,
            Url = row.Url,
            ResourceKind = row.ResourceKind
        };

        switch (type)
        {
            case CourseItemType.Lesson when lessonInfos.TryGetValue(row.SourceId, out var lessonInfo):
                dto.DurationMinutes = lessonInfo.Duration;
                dto.BlocksCount = lessonInfo.BlocksCount;
                break;

            case CourseItemType.Test when testInfos.TryGetValue(row.SourceId, out var testInfo):
                dto.QuestionsCount = testInfo.QuestionsCount;
                dto.Points ??= testInfo.MaxScore > 0 ? testInfo.MaxScore : null;
                dto.Deadline ??= testInfo.Deadline;
                break;

            case CourseItemType.Assignment when assignmentInfos.TryGetValue(row.SourceId, out var assignmentInfo):
                dto.Points ??= assignmentInfo.MaxScore > 0 ? assignmentInfo.MaxScore : null;
                dto.Deadline ??= assignmentInfo.Deadline;
                break;
        }

        return (row.ModuleId, dto);
    }

    // Сервис CourseItemRow инкапсулирует прикладную операцию и скрывает детали инфраструктуры.
    private sealed record CourseItemRow(
        Guid Id,
        Guid? ModuleId,
        string TypeRaw,
        Guid SourceId,
        string Title,
        string? Description,
        string? Url,
        string? ResourceKind,
        int OrderIndex,
        string StatusRaw,
        bool IsRequired,
        decimal? Points,
        DateTime? AvailableFrom,
        DateTime? Deadline);

    // Сервис LessonInfo инкапсулирует прикладную операцию и скрывает детали инфраструктуры.
    private sealed record LessonInfo(int? Duration, int BlocksCount);
    private sealed record TestInfo(int QuestionsCount, int MaxScore, DateTime? Deadline);
    private sealed record AssignmentInfo(int MaxScore, DateTime? Deadline);
}
