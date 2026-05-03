using Assignments.Infrastructure.Persistence;
using Content.Infrastructure.Persistence;
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Scheduling.Infrastructure.Persistence;
using Tests.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

public class CourseBuilderReadService
{
    private const string TypeLesson = "Lesson";
    private const string TypeTest = "Test";
    private const string TypeAssignment = "Assignment";
    private const string TypeLiveSession = "LiveSession";

    private readonly CoursesDbContext _coursesDb;
    private readonly ContentDbContext _contentDb;
    private readonly TestsDbContext _testsDb;
    private readonly AssignmentsDbContext _assignmentsDb;
    private readonly SchedulingDbContext _schedulingDb;

    public CourseBuilderReadService(
        CoursesDbContext coursesDb,
        ContentDbContext contentDb,
        TestsDbContext testsDb,
        AssignmentsDbContext assignmentsDb,
        SchedulingDbContext schedulingDb)
    {
        _coursesDb = coursesDb;
        _contentDb = contentDb;
        _testsDb = testsDb;
        _assignmentsDb = assignmentsDb;
        _schedulingDb = schedulingDb;
    }

    public async Task<CourseBuilderReadResult> GetAsync(
        Guid courseId,
        string userId,
        bool canViewAnyCourse,
        CancellationToken cancellationToken = default)
    {
        var course = await _coursesDb.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseRow(
                c.Id,
                c.DisciplineId,
                c.Discipline.Name,
                c.TeacherId,
                c.TeacherName,
                c.Title,
                c.Description,
                c.ImageUrl,
                c.Level,
                c.Price,
                c.IsFree,
                c.IsPublished,
                c.IsArchived,
                c.ArchiveReason,
                c.OrderType.ToString(),
                c.HasGrading,
                c.HasCertificate,
                c.Deadline,
                c.Tags,
                c.CreatedAt,
                c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)))
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return new CourseBuilderReadResult(CourseBuilderReadStatus.NotFound);

        if (!canViewAnyCourse && course.TeacherId != userId)
            return new CourseBuilderReadResult(CourseBuilderReadStatus.Forbidden);

        var sections = await LoadSectionsAsync(courseId, cancellationToken);
        var courseItems = await LoadCourseItemsAsync(courseId, cancellationToken);
        var lessons = await LoadLessonsAsync(sections.Select(s => s.Id).ToList(), cancellationToken);
        var lessonItems = await BuildLessonItemsAsync(lessons, cancellationToken);
        var testItems = await BuildTestItemsAsync(courseId, cancellationToken);
        var assignmentItems = await BuildAssignmentItemsAsync(courseId, cancellationToken);
        var liveSessionItems = await BuildLiveSessionItemsAsync(courseId, cancellationToken);
        var resourceItems = BuildStandaloneCourseItems(courseItems);

        var sourceItems = lessonItems
            .Concat(testItems)
            .Concat(assignmentItems)
            .Concat(liveSessionItems)
            .ToList();

        ApplyCourseItemMetadata(sourceItems, courseItems);

        var allCourseItems = sourceItems
            .Concat(resourceItems)
            .ToList();

        var itemsBySection = allCourseItems
            .Where(i => i.SectionId.HasValue)
            .GroupBy(i => i.SectionId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(i => i.OrderIndex).ThenBy(i => i.Title).ToList());

        var sectionDtos = sections
            .OrderBy(s => s.OrderIndex)
            .Select(s => new CourseBuilderSectionDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                OrderIndex = s.OrderIndex,
                IsPublished = s.IsPublished,
                Items = itemsBySection.TryGetValue(s.Id, out var items)
                    ? items
                    : new List<CourseBuilderItemDto>()
            })
            .ToList();

        var unsectionedItems = allCourseItems
            .Where(i => !i.SectionId.HasValue)
            .OrderBy(i => i.OrderIndex)
            .ThenBy(i => i.Type)
            .ThenBy(i => i.Deadline ?? i.StartTime ?? DateTime.MaxValue)
            .ThenBy(i => i.Title)
            .ToList();

        var allItems = sectionDtos.SelectMany(s => s.Items).Concat(unsectionedItems).ToList();

        var builder = new CourseBuilderDto
        {
            Course = new CourseBuilderCourseDto
            {
                Id = course.Id,
                DisciplineId = course.DisciplineId,
                DisciplineName = course.DisciplineName,
                TeacherId = course.TeacherId,
                TeacherName = course.TeacherName,
                Title = course.Title,
                Description = course.Description,
                ImageUrl = course.ImageUrl,
                Level = course.Level,
                Price = course.Price,
                IsFree = course.IsFree,
                IsPublished = course.IsPublished,
                IsArchived = course.IsArchived,
                ArchiveReason = course.ArchiveReason,
                OrderType = course.OrderType,
                HasGrading = course.HasGrading,
                HasCertificate = course.HasCertificate,
                Deadline = course.Deadline,
                Tags = course.Tags,
                CreatedAt = course.CreatedAt,
                StudentsCount = course.StudentsCount,
                SectionsCount = sectionDtos.Count,
                LessonsCount = lessonItems.Count,
                TestsCount = testItems.Count,
                AssignmentsCount = assignmentItems.Count,
                LiveSessionsCount = liveSessionItems.Count
            },
            Sections = sectionDtos,
            UnsectionedItems = unsectionedItems,
            Readiness = BuildReadiness(course, sectionDtos, allItems)
        };

        return new CourseBuilderReadResult(CourseBuilderReadStatus.Success, builder);
    }

    private async Task<List<SectionRow>> LoadSectionsAsync(Guid courseId, CancellationToken cancellationToken)
    {
        return await _coursesDb.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.OrderIndex)
            .Select(m => new SectionRow(m.Id, m.Title, m.Description, m.OrderIndex, m.IsPublished))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<LessonRow>> LoadLessonsAsync(
        List<Guid> sectionIds,
        CancellationToken cancellationToken)
    {
        if (sectionIds.Count == 0)
            return new List<LessonRow>();

        return await _coursesDb.Lessons
            .AsNoTracking()
            .Where(l => sectionIds.Contains(l.ModuleId))
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonRow(
                l.Id,
                l.ModuleId,
                l.Title,
                l.Description,
                l.OrderIndex,
                l.IsPublished,
                l.Duration,
                l.Layout.ToString()))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<CourseItemRow>> LoadCourseItemsAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        return await _coursesDb.CourseItems
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
                i.AttachmentId,
                i.ResourceKind,
                i.OrderIndex,
                i.Status.ToString(),
                i.IsRequired,
                i.Points,
                i.AvailableFrom,
                i.Deadline))
            .ToListAsync(cancellationToken);
    }

    private static void ApplyCourseItemMetadata(
        List<CourseBuilderItemDto> items,
        List<CourseItemRow> courseItems)
    {
        var itemsBySource = courseItems
            .ToDictionary(i => (i.Type, i.SourceId), i => i);

        foreach (var item in items)
        {
            if (!itemsBySource.TryGetValue((item.Type, item.SourceId), out var courseItem))
                continue;

            item.CourseItemId = courseItem.Id;
            item.SectionId = courseItem.SectionId;
            item.OrderIndex = courseItem.OrderIndex;
            item.IsRequired = courseItem.IsRequired;
            item.AvailableFrom = courseItem.AvailableFrom;
            item.Deadline ??= courseItem.Deadline;
            item.Points ??= courseItem.Points;

            if (item.Status == "Draft" || courseItem.Status == "Archived")
                item.Status = courseItem.Status;
        }
    }

    private static List<CourseBuilderItemDto> BuildStandaloneCourseItems(List<CourseItemRow> courseItems)
    {
        return courseItems
            .Where(i => i.Type is "Resource" or "ExternalLink")
            .Select(i => new CourseBuilderItemDto
            {
                CourseItemId = i.Id,
                SourceId = i.SourceId,
                SectionId = i.SectionId,
                Type = i.Type,
                Title = i.Title,
                Description = i.Description,
                Url = i.Url,
                AttachmentId = i.AttachmentId,
                ResourceKind = i.ResourceKind,
                OrderIndex = i.OrderIndex,
                Status = i.Status,
                IsRequired = i.IsRequired,
                Points = i.Points,
                AvailableFrom = i.AvailableFrom,
                Deadline = i.Deadline
            })
            .ToList();
    }

    private async Task<List<CourseBuilderItemDto>> BuildLessonItemsAsync(
        List<LessonRow> lessons,
        CancellationToken cancellationToken)
    {
        if (lessons.Count == 0)
            return new List<CourseBuilderItemDto>();

        var lessonIds = lessons.Select(l => l.Id).ToList();
        var blockCounts = await _contentDb.LessonBlocks
            .AsNoTracking()
            .Where(b => lessonIds.Contains(b.LessonId))
            .GroupBy(b => b.LessonId)
            .Select(g => new { LessonId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.LessonId, x => x.Count, cancellationToken);

        return lessons
            .Select(l =>
            {
                blockCounts.TryGetValue(l.Id, out var blocksCount);
                return new CourseBuilderItemDto
                {
                    SourceId = l.Id,
                    SectionId = l.SectionId,
                    Type = TypeLesson,
                    Title = l.Title,
                    Description = l.Description,
                    OrderIndex = l.OrderIndex,
                    IsPublished = l.IsPublished,
                    Status = l.IsPublished ? "Published" : blocksCount > 0 ? "Ready" : "NeedsContent",
                    DurationMinutes = l.Duration,
                    BlocksCount = blocksCount
                };
            })
            .ToList();
    }

    private async Task<List<CourseBuilderItemDto>> BuildTestItemsAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var tests = await _testsDb.Tests
            .AsNoTracking()
            .Where(t => t.CourseId == courseId)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new TestRow(
                t.Id,
                t.Title,
                t.Description,
                t.Deadline,
                t.MaxScore,
                t.Questions.Count))
            .ToListAsync(cancellationToken);

        if (tests.Count == 0)
            return new List<CourseBuilderItemDto>();

        var testIds = tests.Select(t => t.Id).ToList();
        var attemptCounts = await _testsDb.TestAttempts
            .AsNoTracking()
            .Where(a => testIds.Contains(a.TestId))
            .GroupBy(a => a.TestId)
            .Select(g => new { TestId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TestId, x => x.Count, cancellationToken);

        return tests.Select(t =>
        {
            attemptCounts.TryGetValue(t.Id, out var attemptsCount);
            return new CourseBuilderItemDto
            {
                SourceId = t.Id,
                Type = TypeTest,
                Title = t.Title,
                Description = t.Description,
                Status = t.QuestionsCount > 0 ? "Ready" : "NeedsContent",
                Points = t.MaxScore > 0 ? t.MaxScore : null,
                Deadline = t.Deadline,
                QuestionsCount = t.QuestionsCount,
                AttemptsCount = attemptsCount
            };
        }).ToList();
    }

    private async Task<List<CourseBuilderItemDto>> BuildAssignmentItemsAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var assignments = await _assignmentsDb.Assignments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AssignmentRow(
                a.Id,
                a.Title,
                a.Description,
                a.Deadline,
                a.MaxScore,
                a.Submissions.Count))
            .ToListAsync(cancellationToken);

        return assignments.Select(a => new CourseBuilderItemDto
        {
            SourceId = a.Id,
            Type = TypeAssignment,
            Title = a.Title,
            Description = a.Description,
            Status = !string.IsNullOrWhiteSpace(a.Description) && a.MaxScore > 0 ? "Ready" : "NeedsContent",
            Points = a.MaxScore > 0 ? a.MaxScore : null,
            Deadline = a.Deadline,
            SubmissionsCount = a.SubmissionsCount
        }).ToList();
    }

    private async Task<List<CourseBuilderItemDto>> BuildLiveSessionItemsAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var slots = await _schedulingDb.ScheduleSlots
            .AsNoTracking()
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.StartTime)
            .Select(s => new LiveSessionRow(
                s.Id,
                s.Title,
                s.Description,
                s.StartTime,
                s.EndTime,
                s.MaxStudents,
                s.MeetingLink,
                s.Bookings.Count))
            .ToListAsync(cancellationToken);

        return slots.Select(s => new CourseBuilderItemDto
        {
            SourceId = s.Id,
            Type = TypeLiveSession,
            Title = s.Title,
            Description = s.Description,
            Status = s.StartTime < s.EndTime ? "Ready" : "NeedsContent",
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            DurationMinutes = s.EndTime > s.StartTime ? (int)(s.EndTime - s.StartTime).TotalMinutes : null,
            MaxStudents = s.MaxStudents,
            BookedCount = s.BookedCount,
            MeetingLink = s.MeetingLink
        }).ToList();
    }

    private static CourseBuilderReadinessDto BuildReadiness(
        CourseRow course,
        List<CourseBuilderSectionDto> sections,
        List<CourseBuilderItemDto> items)
    {
        var issues = new List<CourseBuilderReadinessIssueDto>();

        if (string.IsNullOrWhiteSpace(course.Title))
            issues.Add(Error("COURSE_TITLE_EMPTY", "У курса не заполнено название."));

        if (string.IsNullOrWhiteSpace(course.Description))
            issues.Add(Error("COURSE_DESCRIPTION_EMPTY", "У курса не заполнено описание."));

        if (sections.Count == 0)
            issues.Add(Error("NO_SECTIONS", "В курсе нет ни одного раздела."));

        if (items.Count == 0)
            issues.Add(Error("NO_ITEMS", "В курсе нет ни одного учебного элемента."));

        foreach (var section in sections)
        {
            if (section.Items.Count == 0)
            {
                issues.Add(Warning(
                    "EMPTY_SECTION",
                    $"В разделе «{section.Title}» пока нет элементов.",
                    sectionId: section.Id));
            }
        }

        foreach (var item in items)
        {
            switch (item.Type)
            {
                case TypeLesson when (item.BlocksCount ?? 0) == 0:
                    issues.Add(Warning(
                        "LESSON_HAS_NO_BLOCKS",
                        $"В уроке «{item.Title}» нет блоков.",
                        item.Type,
                        item.SourceId,
                        item.SectionId));
                    break;

                case TypeTest when (item.QuestionsCount ?? 0) == 0:
                    issues.Add(Error(
                        "TEST_HAS_NO_QUESTIONS",
                        $"В тесте «{item.Title}» нет вопросов.",
                        item.Type,
                        item.SourceId,
                        item.SectionId));
                    break;

                case TypeAssignment:
                    if (string.IsNullOrWhiteSpace(item.Description))
                    {
                        issues.Add(Error(
                            "ASSIGNMENT_DESCRIPTION_EMPTY",
                            $"В задании «{item.Title}» не заполнено описание.",
                            item.Type,
                            item.SourceId,
                            item.SectionId));
                    }
                    if (!item.Points.HasValue || item.Points <= 0)
                    {
                        issues.Add(Error(
                            "ASSIGNMENT_POINTS_EMPTY",
                            $"В задании «{item.Title}» не указан максимальный балл.",
                            item.Type,
                            item.SourceId,
                            item.SectionId));
                    }
                    break;

                case TypeLiveSession:
                    if (!item.StartTime.HasValue || !item.EndTime.HasValue || item.StartTime >= item.EndTime)
                    {
                        issues.Add(Error(
                            "LIVE_SESSION_TIME_INVALID",
                            $"У live-занятия «{item.Title}» некорректное время.",
                            item.Type,
                            item.SourceId,
                            item.SectionId));
                    }
                    if (string.IsNullOrWhiteSpace(item.MeetingLink))
                    {
                        issues.Add(Warning(
                            "LIVE_SESSION_MEETING_LINK_EMPTY",
                            $"У live-занятия «{item.Title}» не указана ссылка на встречу.",
                            item.Type,
                            item.SourceId,
                            item.SectionId));
                    }
                    break;
            }
        }

        var readyItems = items.Count(i => i.Status is "Ready" or "Published");
        var errorCount = issues.Count(i => i.Severity == "Error");
        var warningCount = issues.Count(i => i.Severity == "Warning");

        return new CourseBuilderReadinessDto
        {
            TotalItems = items.Count,
            ReadyItems = readyItems,
            ReadyPercent = items.Count == 0 ? 0 : Math.Round((decimal)readyItems / items.Count * 100m, 2),
            ErrorCount = errorCount,
            WarningCount = warningCount,
            Issues = issues
        };
    }

    private static CourseBuilderReadinessIssueDto Error(
        string code,
        string message,
        string? itemType = null,
        Guid? sourceId = null,
        Guid? sectionId = null)
    {
        return new CourseBuilderReadinessIssueDto
        {
            Severity = "Error",
            Code = code,
            Message = message,
            ItemType = itemType,
            SourceId = sourceId,
            SectionId = sectionId
        };
    }

    private static CourseBuilderReadinessIssueDto Warning(
        string code,
        string message,
        string? itemType = null,
        Guid? sourceId = null,
        Guid? sectionId = null)
    {
        return new CourseBuilderReadinessIssueDto
        {
            Severity = "Warning",
            Code = code,
            Message = message,
            ItemType = itemType,
            SourceId = sourceId,
            SectionId = sectionId
        };
    }

    private sealed record CourseRow(
        Guid Id,
        Guid DisciplineId,
        string DisciplineName,
        string TeacherId,
        string TeacherName,
        string Title,
        string Description,
        string? ImageUrl,
        CourseLevel Level,
        decimal? Price,
        bool IsFree,
        bool IsPublished,
        bool IsArchived,
        string? ArchiveReason,
        string OrderType,
        bool HasGrading,
        bool HasCertificate,
        DateTime? Deadline,
        string? Tags,
        DateTime CreatedAt,
        int StudentsCount);

    private sealed record SectionRow(
        Guid Id,
        string Title,
        string? Description,
        int OrderIndex,
        bool IsPublished);

    private sealed record LessonRow(
        Guid Id,
        Guid SectionId,
        string Title,
        string? Description,
        int OrderIndex,
        bool IsPublished,
        int? Duration,
        string Layout);

    private sealed record CourseItemRow(
        Guid Id,
        Guid? SectionId,
        string Type,
        Guid SourceId,
        string Title,
        string? Description,
        string? Url,
        Guid? AttachmentId,
        string? ResourceKind,
        int OrderIndex,
        string Status,
        bool IsRequired,
        decimal? Points,
        DateTime? AvailableFrom,
        DateTime? Deadline);

    private sealed record TestRow(
        Guid Id,
        string Title,
        string? Description,
        DateTime? Deadline,
        int MaxScore,
        int QuestionsCount);

    private sealed record AssignmentRow(
        Guid Id,
        string Title,
        string Description,
        DateTime? Deadline,
        int MaxScore,
        int SubmissionsCount);

    private sealed record LiveSessionRow(
        Guid Id,
        string Title,
        string? Description,
        DateTime StartTime,
        DateTime EndTime,
        int MaxStudents,
        string? MeetingLink,
        int BookedCount);
}
