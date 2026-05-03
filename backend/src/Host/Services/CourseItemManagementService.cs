using Assignments.Infrastructure.Persistence;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Models.Courses;
using Microsoft.EntityFrameworkCore;
using Scheduling.Infrastructure.Persistence;
using Tests.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

public class CourseItemManagementService
{
    private readonly CoursesDbContext _coursesDb;
    private readonly TestsDbContext _testsDb;
    private readonly AssignmentsDbContext _assignmentsDb;
    private readonly SchedulingDbContext _schedulingDb;

    public CourseItemManagementService(
        CoursesDbContext coursesDb,
        TestsDbContext testsDb,
        AssignmentsDbContext assignmentsDb,
        SchedulingDbContext schedulingDb)
    {
        _coursesDb = coursesDb;
        _testsDb = testsDb;
        _assignmentsDb = assignmentsDb;
        _schedulingDb = schedulingDb;
    }

    public async Task<CourseItemMutationResult<CourseItemBackfillDto>> BackfillAsync(
        Guid courseId,
        string userId,
        bool canManageAnyCourse,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanManageCourseAsync(courseId, userId, canManageAnyCourse, cancellationToken);
        if (access != CourseItemMutationStatus.Success)
            return new CourseItemMutationResult<CourseItemBackfillDto>(access);

        var existingKeys = await _coursesDb.CourseItems
            .Where(i => i.CourseId == courseId)
            .Select(i => new { i.Type, i.SourceId })
            .ToListAsync(cancellationToken);

        var existing = existingKeys
            .Select(i => (i.Type, i.SourceId))
            .ToHashSet();

        var result = new CourseItemBackfillDto();

        result.LessonsCount = await BackfillLessonsAsync(courseId, existing, cancellationToken);
        if (result.LessonsCount > 0)
            await _coursesDb.SaveChangesAsync(cancellationToken);

        result.TestsCount = await BackfillTestsAsync(courseId, existing, cancellationToken);
        if (result.TestsCount > 0)
            await _coursesDb.SaveChangesAsync(cancellationToken);

        result.AssignmentsCount = await BackfillAssignmentsAsync(courseId, existing, cancellationToken);
        if (result.AssignmentsCount > 0)
            await _coursesDb.SaveChangesAsync(cancellationToken);

        result.LiveSessionsCount = await BackfillLiveSessionsAsync(courseId, existing, cancellationToken);
        if (result.LiveSessionsCount > 0)
            await _coursesDb.SaveChangesAsync(cancellationToken);

        result.CreatedItemsCount = result.LessonsCount
            + result.TestsCount
            + result.AssignmentsCount
            + result.LiveSessionsCount;

        return new CourseItemMutationResult<CourseItemBackfillDto>(CourseItemMutationStatus.Success, result);
    }

    public async Task<CourseItemMutationResult<CourseItemDto>> MoveAsync(
        Guid courseId,
        Guid itemId,
        MoveCourseItemRequest request,
        string userId,
        bool canManageAnyCourse,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanManageCourseAsync(courseId, userId, canManageAnyCourse, cancellationToken);
        if (access != CourseItemMutationStatus.Success)
            return new CourseItemMutationResult<CourseItemDto>(access);

        if (request.SectionId.HasValue)
        {
            var sectionExists = await _coursesDb.CourseModules
                .AnyAsync(m => m.Id == request.SectionId.Value && m.CourseId == courseId, cancellationToken);

            if (!sectionExists)
            {
                return new CourseItemMutationResult<CourseItemDto>(
                    CourseItemMutationStatus.ValidationFailed,
                    Error: "Раздел курса не найден.");
            }
        }

        var items = await _coursesDb.CourseItems
            .Where(i => i.CourseId == courseId)
            .OrderBy(i => i.OrderIndex)
            .ToListAsync(cancellationToken);

        var item = items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return new CourseItemMutationResult<CourseItemDto>(CourseItemMutationStatus.NotFound);

        var oldSectionId = item.ModuleId;
        var newSectionId = request.SectionId;
        var newOrderIndex = Math.Max(0, request.OrderIndex);

        if (oldSectionId != newSectionId)
            NormalizeArea(items, oldSectionId, excludeItemId: item.Id);

        item.ModuleId = newSectionId;
        MoveWithinArea(items, item, newSectionId, newOrderIndex);

        await _coursesDb.SaveChangesAsync(cancellationToken);
        return new CourseItemMutationResult<CourseItemDto>(CourseItemMutationStatus.Success, ToDto(item));
    }

    public async Task<CourseItemMutationResult<List<CourseItemDto>>> ReorderAsync(
        Guid courseId,
        ReorderCourseItemsRequest request,
        string userId,
        bool canManageAnyCourse,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanManageCourseAsync(courseId, userId, canManageAnyCourse, cancellationToken);
        if (access != CourseItemMutationStatus.Success)
            return new CourseItemMutationResult<List<CourseItemDto>>(access);

        if (request.SectionId.HasValue)
        {
            var sectionExists = await _coursesDb.CourseModules
                .AnyAsync(m => m.Id == request.SectionId.Value && m.CourseId == courseId, cancellationToken);

            if (!sectionExists)
            {
                return new CourseItemMutationResult<List<CourseItemDto>>(
                    CourseItemMutationStatus.ValidationFailed,
                    Error: "Раздел курса не найден.");
            }
        }

        var items = await _coursesDb.CourseItems
            .Where(i => i.CourseId == courseId && i.ModuleId == request.SectionId)
            .OrderBy(i => i.OrderIndex)
            .ToListAsync(cancellationToken);

        var ids = request.ItemIds.Distinct().ToList();
        if (ids.Count != request.ItemIds.Count)
        {
            return new CourseItemMutationResult<List<CourseItemDto>>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Список элементов содержит дубликаты.");
        }

        var areaIds = items.Select(i => i.Id).ToHashSet();
        if (ids.Any(id => !areaIds.Contains(id)))
        {
            return new CourseItemMutationResult<List<CourseItemDto>>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Один или несколько элементов не находятся в указанном разделе.");
        }

        var byId = items.ToDictionary(i => i.Id);
        var ordered = ids.Select(id => byId[id])
            .Concat(items.Where(i => !ids.Contains(i.Id)).OrderBy(i => i.OrderIndex))
            .ToList();

        for (var index = 0; index < ordered.Count; index++)
            ordered[index].OrderIndex = index;

        await _coursesDb.SaveChangesAsync(cancellationToken);
        return new CourseItemMutationResult<List<CourseItemDto>>(
            CourseItemMutationStatus.Success,
            ordered.Select(ToDto).ToList());
    }

    public async Task<CourseItemMutationResult<CourseItemDto>> CreateStandaloneAsync(
        Guid courseId,
        CreateStandaloneCourseItemRequest request,
        string userId,
        bool canManageAnyCourse,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanManageCourseAsync(courseId, userId, canManageAnyCourse, cancellationToken);
        if (access != CourseItemMutationStatus.Success)
            return new CourseItemMutationResult<CourseItemDto>(access);

        var validationError = await ValidateStandaloneRequestAsync(courseId, request, cancellationToken);
        if (validationError is not null)
        {
            return new CourseItemMutationResult<CourseItemDto>(
                CourseItemMutationStatus.ValidationFailed,
                Error: validationError);
        }

        var itemId = Guid.NewGuid();
        var item = new CourseItem
        {
            Id = itemId,
            CourseId = courseId,
            ModuleId = request.SectionId,
            Type = request.Type,
            SourceId = itemId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Url = request.Url,
            AttachmentId = request.AttachmentId,
            ResourceKind = request.ResourceKind,
            OrderIndex = await GetNextOrderIndexAsync(courseId, request.SectionId, cancellationToken),
            Status = CourseItemStatus.Ready,
            IsRequired = request.IsRequired,
            Points = request.Points,
            AvailableFrom = request.AvailableFrom,
            Deadline = request.Deadline
        };

        _coursesDb.CourseItems.Add(item);
        await _coursesDb.SaveChangesAsync(cancellationToken);
        return new CourseItemMutationResult<CourseItemDto>(CourseItemMutationStatus.Success, ToDto(item));
    }

    public async Task<CourseItemMutationResult<CourseItemDto>> UpdateStandaloneAsync(
        Guid courseId,
        Guid itemId,
        UpdateStandaloneCourseItemRequest request,
        string userId,
        bool canManageAnyCourse,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanManageCourseAsync(courseId, userId, canManageAnyCourse, cancellationToken);
        if (access != CourseItemMutationStatus.Success)
            return new CourseItemMutationResult<CourseItemDto>(access);

        var item = await _coursesDb.CourseItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.CourseId == courseId, cancellationToken);

        if (item is null)
            return new CourseItemMutationResult<CourseItemDto>(CourseItemMutationStatus.NotFound);

        if (!IsStandaloneType(item.Type))
        {
            return new CourseItemMutationResult<CourseItemDto>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Редактировать содержимое можно только у материала или внешней ссылки.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return new CourseItemMutationResult<CourseItemDto>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Название элемента обязательно.");
        }

        if (item.Type == CourseItemType.ExternalLink && string.IsNullOrWhiteSpace(request.Url))
        {
            return new CourseItemMutationResult<CourseItemDto>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Для внешней ссылки нужен URL.");
        }

        if (item.Type == CourseItemType.Resource
            && !request.AttachmentId.HasValue
            && string.IsNullOrWhiteSpace(request.Url))
        {
            return new CourseItemMutationResult<CourseItemDto>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Для материала нужен файл или URL.");
        }

        item.Title = request.Title.Trim();
        item.Description = request.Description;
        item.Url = request.Url;
        item.AttachmentId = request.AttachmentId;
        item.ResourceKind = request.ResourceKind;

        await _coursesDb.SaveChangesAsync(cancellationToken);
        return new CourseItemMutationResult<CourseItemDto>(CourseItemMutationStatus.Success, ToDto(item));
    }

    public async Task<CourseItemMutationResult<string>> DeleteStandaloneAsync(
        Guid courseId,
        Guid itemId,
        string userId,
        bool canManageAnyCourse,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanManageCourseAsync(courseId, userId, canManageAnyCourse, cancellationToken);
        if (access != CourseItemMutationStatus.Success)
            return new CourseItemMutationResult<string>(access);

        var item = await _coursesDb.CourseItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.CourseId == courseId, cancellationToken);

        if (item is null)
            return new CourseItemMutationResult<string>(CourseItemMutationStatus.NotFound);

        if (!IsStandaloneType(item.Type))
        {
            return new CourseItemMutationResult<string>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Удалять через Course Builder можно только материал или внешнюю ссылку. Уроки, тесты и задания удаляются через свои разделы.");
        }

        var sectionId = item.ModuleId;
        var items = await _coursesDb.CourseItems
            .Where(i => i.CourseId == courseId)
            .ToListAsync(cancellationToken);

        _coursesDb.CourseItems.Remove(item);
        NormalizeArea(items, sectionId, excludeItemId: item.Id);

        await _coursesDb.SaveChangesAsync(cancellationToken);
        return new CourseItemMutationResult<string>(CourseItemMutationStatus.Success, "Элемент удалён.");
    }

    public async Task<CourseItemMutationResult<CourseItemDto>> UpdateMetadataAsync(
        Guid courseId,
        Guid itemId,
        UpdateCourseItemMetadataRequest request,
        string userId,
        bool canManageAnyCourse,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanManageCourseAsync(courseId, userId, canManageAnyCourse, cancellationToken);
        if (access != CourseItemMutationStatus.Success)
            return new CourseItemMutationResult<CourseItemDto>(access);

        if (request.Points.HasValue && request.Points.Value < 0)
        {
            return new CourseItemMutationResult<CourseItemDto>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Баллы не могут быть отрицательными.");
        }

        if (request.AvailableFrom.HasValue
            && request.Deadline.HasValue
            && request.AvailableFrom.Value > request.Deadline.Value)
        {
            return new CourseItemMutationResult<CourseItemDto>(
                CourseItemMutationStatus.ValidationFailed,
                Error: "Дата доступности не может быть позже дедлайна.");
        }

        var item = await _coursesDb.CourseItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.CourseId == courseId, cancellationToken);

        if (item is null)
            return new CourseItemMutationResult<CourseItemDto>(CourseItemMutationStatus.NotFound);

        item.IsRequired = request.IsRequired;
        item.Points = request.Points;
        item.AvailableFrom = request.AvailableFrom;
        item.Deadline = request.Deadline;
        item.Status = request.Status;

        await _coursesDb.SaveChangesAsync(cancellationToken);
        return new CourseItemMutationResult<CourseItemDto>(CourseItemMutationStatus.Success, ToDto(item));
    }

    private async Task<CourseItemMutationStatus> EnsureCanManageCourseAsync(
        Guid courseId,
        string userId,
        bool canManageAnyCourse,
        CancellationToken cancellationToken)
    {
        var course = await _coursesDb.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.TeacherId })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return CourseItemMutationStatus.NotFound;

        if (!canManageAnyCourse && course.TeacherId != userId)
            return CourseItemMutationStatus.Forbidden;

        return CourseItemMutationStatus.Success;
    }

    private async Task<string?> ValidateStandaloneRequestAsync(
        Guid courseId,
        CreateStandaloneCourseItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsStandaloneType(request.Type))
            return "Через этот endpoint можно создать только материал или внешнюю ссылку.";

        if (string.IsNullOrWhiteSpace(request.Title))
            return "Название элемента обязательно.";

        if (request.Points.HasValue && request.Points.Value < 0)
            return "Баллы не могут быть отрицательными.";

        if (request.AvailableFrom.HasValue
            && request.Deadline.HasValue
            && request.AvailableFrom.Value > request.Deadline.Value)
        {
            return "Дата доступности не может быть позже дедлайна.";
        }

        if (request.SectionId.HasValue)
        {
            var sectionExists = await _coursesDb.CourseModules
                .AnyAsync(m => m.Id == request.SectionId.Value && m.CourseId == courseId, cancellationToken);

            if (!sectionExists)
                return "Раздел курса не найден.";
        }

        if (request.Type == CourseItemType.ExternalLink && string.IsNullOrWhiteSpace(request.Url))
            return "Для внешней ссылки нужен URL.";

        if (request.Type == CourseItemType.Resource
            && !request.AttachmentId.HasValue
            && string.IsNullOrWhiteSpace(request.Url))
        {
            return "Для материала нужен файл или URL.";
        }

        return null;
    }

    private static bool IsStandaloneType(CourseItemType type)
    {
        return type is CourseItemType.Resource or CourseItemType.ExternalLink;
    }

    private async Task<int> BackfillLessonsAsync(
        Guid courseId,
        HashSet<(CourseItemType Type, Guid SourceId)> existing,
        CancellationToken cancellationToken)
    {
        var lessons = await _coursesDb.Lessons
            .AsNoTracking()
            .Where(l => l.Module.CourseId == courseId)
            .OrderBy(l => l.Module.OrderIndex)
            .ThenBy(l => l.OrderIndex)
            .Select(l => new
            {
                l.Id,
                l.ModuleId,
                l.Title,
                l.Description,
                l.OrderIndex,
                l.IsPublished
            })
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var lesson in lessons)
        {
            if (!existing.Add((CourseItemType.Lesson, lesson.Id)))
                continue;

            _coursesDb.CourseItems.Add(new CourseItem
            {
                CourseId = courseId,
                ModuleId = lesson.ModuleId,
                Type = CourseItemType.Lesson,
                SourceId = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                OrderIndex = lesson.OrderIndex,
                Status = lesson.IsPublished ? CourseItemStatus.Published : CourseItemStatus.Draft
            });
            created++;
        }

        return created;
    }

    private async Task<int> BackfillTestsAsync(
        Guid courseId,
        HashSet<(CourseItemType Type, Guid SourceId)> existing,
        CancellationToken cancellationToken)
    {
        var nextOrder = await GetNextOrderIndexAsync(courseId, moduleId: null, cancellationToken);
        var tests = await _testsDb.Tests
            .AsNoTracking()
            .Where(t => t.CourseId == courseId)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.MaxScore,
                t.Deadline,
                QuestionsCount = t.Questions.Count
            })
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var test in tests)
        {
            if (!existing.Add((CourseItemType.Test, test.Id)))
                continue;

            _coursesDb.CourseItems.Add(new CourseItem
            {
                CourseId = courseId,
                Type = CourseItemType.Test,
                SourceId = test.Id,
                Title = test.Title,
                Description = test.Description,
                OrderIndex = nextOrder++,
                Status = test.QuestionsCount > 0 ? CourseItemStatus.Ready : CourseItemStatus.NeedsContent,
                Points = test.MaxScore > 0 ? test.MaxScore : null,
                Deadline = test.Deadline
            });
            created++;
        }

        return created;
    }

    private async Task<int> BackfillAssignmentsAsync(
        Guid courseId,
        HashSet<(CourseItemType Type, Guid SourceId)> existing,
        CancellationToken cancellationToken)
    {
        var nextOrder = await GetNextOrderIndexAsync(courseId, moduleId: null, cancellationToken);
        var assignments = await _assignmentsDb.Assignments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.MaxScore,
                a.Deadline
            })
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var assignment in assignments)
        {
            if (!existing.Add((CourseItemType.Assignment, assignment.Id)))
                continue;

            _coursesDb.CourseItems.Add(new CourseItem
            {
                CourseId = courseId,
                Type = CourseItemType.Assignment,
                SourceId = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                OrderIndex = nextOrder++,
                Status = !string.IsNullOrWhiteSpace(assignment.Description) && assignment.MaxScore > 0
                    ? CourseItemStatus.Ready
                    : CourseItemStatus.NeedsContent,
                Points = assignment.MaxScore > 0 ? assignment.MaxScore : null,
                Deadline = assignment.Deadline
            });
            created++;
        }

        return created;
    }

    private async Task<int> BackfillLiveSessionsAsync(
        Guid courseId,
        HashSet<(CourseItemType Type, Guid SourceId)> existing,
        CancellationToken cancellationToken)
    {
        var nextOrder = await GetNextOrderIndexAsync(courseId, moduleId: null, cancellationToken);
        var slots = await _schedulingDb.ScheduleSlots
            .AsNoTracking()
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.StartTime)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Description,
                s.StartTime,
                s.EndTime
            })
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var slot in slots)
        {
            if (!existing.Add((CourseItemType.LiveSession, slot.Id)))
                continue;

            _coursesDb.CourseItems.Add(new CourseItem
            {
                CourseId = courseId,
                Type = CourseItemType.LiveSession,
                SourceId = slot.Id,
                Title = slot.Title,
                Description = slot.Description,
                OrderIndex = nextOrder++,
                Status = slot.StartTime < slot.EndTime ? CourseItemStatus.Ready : CourseItemStatus.NeedsContent,
                Deadline = slot.StartTime
            });
            created++;
        }

        return created;
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

    private static void NormalizeArea(
        List<CourseItem> items,
        Guid? sectionId,
        Guid excludeItemId)
    {
        var ordered = items
            .Where(i => i.ModuleId == sectionId && i.Id != excludeItemId)
            .OrderBy(i => i.OrderIndex)
            .ThenBy(i => i.CreatedAt)
            .ToList();

        for (var index = 0; index < ordered.Count; index++)
            ordered[index].OrderIndex = index;
    }

    private static void MoveWithinArea(
        List<CourseItem> items,
        CourseItem item,
        Guid? sectionId,
        int targetIndex)
    {
        var ordered = items
            .Where(i => i.ModuleId == sectionId && i.Id != item.Id)
            .OrderBy(i => i.OrderIndex)
            .ThenBy(i => i.CreatedAt)
            .ToList();

        var insertIndex = Math.Clamp(targetIndex, 0, ordered.Count);
        ordered.Insert(insertIndex, item);

        for (var index = 0; index < ordered.Count; index++)
            ordered[index].OrderIndex = index;
    }

    private static CourseItemDto ToDto(CourseItem item)
    {
        return new CourseItemDto
        {
            Id = item.Id,
            CourseId = item.CourseId,
            SectionId = item.ModuleId,
            Type = item.Type,
            SourceId = item.SourceId,
            Title = item.Title,
            Description = item.Description,
            Url = item.Url,
            AttachmentId = item.AttachmentId,
            ResourceKind = item.ResourceKind,
            OrderIndex = item.OrderIndex,
            Status = item.Status,
            IsRequired = item.IsRequired,
            Points = item.Points,
            AvailableFrom = item.AvailableFrom,
            Deadline = item.Deadline
        };
    }
}
