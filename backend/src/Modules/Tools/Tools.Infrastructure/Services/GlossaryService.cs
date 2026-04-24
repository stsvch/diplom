using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Tools.Application.DTOs;
using Tools.Application.Interfaces;
using Tools.Domain.Entities;
using Tools.Domain.Enums;

namespace Tools.Infrastructure.Services;

public class GlossaryService : IGlossaryService
{
    private readonly IToolsDbContext _toolsDb;
    private readonly ICoursesDbContext _coursesDb;

    public GlossaryService(IToolsDbContext toolsDb, ICoursesDbContext coursesDb)
    {
        _toolsDb = toolsDb;
        _coursesDb = coursesDb;
    }

    public async Task<IReadOnlyList<DictionaryWordDto>> GetTeacherWordsAsync(
        string teacherId,
        Guid? courseId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var accessibleCourses = await _coursesDb.Courses
            .AsNoTracking()
            .Where(course => course.TeacherId == teacherId)
            .Select(course => new { course.Id, course.Title })
            .ToListAsync(cancellationToken);

        if (accessibleCourses.Count == 0)
            return [];

        var accessibleCourseIds = accessibleCourses.Select(course => course.Id).ToList();
        var courseTitles = accessibleCourses.ToDictionary(course => course.Id, course => course.Title);

        var words = await BuildWordQuery(accessibleCourseIds, courseId, search)
            .OrderBy(word => word.Term)
            .ThenBy(word => word.CreatedAt)
            .ToListAsync(cancellationToken);

        return words
            .Select(word => MapWord(word, courseTitles.GetValueOrDefault(word.CourseId) ?? string.Empty, null))
            .ToList();
    }

    public async Task<IReadOnlyList<DictionaryWordDto>> GetStudentWordsAsync(
        string studentId,
        Guid? courseId,
        string? search,
        bool knownOnly,
        CancellationToken cancellationToken = default)
    {
        var accessibleCourseIds = await GetStudentAccessibleCourseIdsAsync(studentId, cancellationToken);

        if (accessibleCourseIds.Count == 0)
            return [];

        var courseTitles = await GetCourseTitlesAsync(accessibleCourseIds, cancellationToken);

        var words = await BuildWordQuery(accessibleCourseIds, courseId, search)
            .OrderBy(word => word.Term)
            .ThenBy(word => word.CreatedAt)
            .ToListAsync(cancellationToken);

        if (words.Count == 0)
            return [];

        var wordIds = words.Select(word => word.Id).ToList();
        var progressMap = await _toolsDb.UserDictionaryProgress
            .AsNoTracking()
            .Where(progress => progress.UserId == studentId && wordIds.Contains(progress.WordId))
            .ToDictionaryAsync(progress => progress.WordId, progress => progress, cancellationToken);

        var result = words
            .Select(word =>
            {
                progressMap.TryGetValue(word.Id, out var progress);
                return MapWord(word, courseTitles.GetValueOrDefault(word.CourseId) ?? string.Empty, progress);
            })
            .ToList();

        if (knownOnly)
        {
            result = result.Where(word => word.IsKnown).ToList();
        }

        return result;
    }

    public async Task<IReadOnlyList<DictionaryWordDto>> GetStudentReviewSessionAsync(
        string studentId,
        Guid? courseId,
        int take,
        IReadOnlyCollection<Guid>? excludeWordIds,
        CancellationToken cancellationToken = default)
    {
        var accessibleCourseIds = await GetStudentAccessibleCourseIdsAsync(studentId, cancellationToken);
        if (accessibleCourseIds.Count == 0)
            return [];

        var safeTake = Math.Clamp(take, 1, 50);
        var courseTitles = await GetCourseTitlesAsync(accessibleCourseIds, cancellationToken);
        var excludedIds = excludeWordIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToHashSet() ?? [];

        var words = await BuildWordQuery(accessibleCourseIds, courseId, null)
            .Where(word => !excludedIds.Contains(word.Id))
            .ToListAsync(cancellationToken);

        if (words.Count == 0)
            return [];

        var progressMap = await LoadProgressMapAsync(studentId, words.Select(word => word.Id), cancellationToken);
        var now = DateTime.UtcNow;

        return words
            .Select(word =>
            {
                progressMap.TryGetValue(word.Id, out var progress);
                return MapWord(word, courseTitles.GetValueOrDefault(word.CourseId) ?? string.Empty, progress);
            })
            .Where(word => !word.NextReviewAt.HasValue || word.NextReviewAt.Value <= now)
            .OrderBy(word => word.NextReviewAt.HasValue ? 0 : 1)
            .ThenBy(word => word.NextReviewAt ?? DateTime.MinValue)
            .ThenBy(word => word.IsKnown ? 1 : 0)
            .ThenBy(word => word.LastReviewedAt ?? DateTime.MinValue)
            .ThenBy(word => word.ReviewCount)
            .ThenBy(word => word.Term)
            .Take(safeTake)
            .ToList();
    }

    public async Task<DictionaryWordDto> CreateWordAsync(
        string teacherId,
        Guid courseId,
        string term,
        string translation,
        string? definition,
        string? example,
        IReadOnlyCollection<string>? tags,
        CancellationToken cancellationToken = default)
    {
        var courseTitle = await EnsureTeacherCourseAccessAsync(courseId, teacherId, cancellationToken);

        var word = new DictionaryWord
        {
            CourseId = courseId,
            Term = NormalizeRequired(term, nameof(term), 200),
            Translation = NormalizeRequired(translation, nameof(translation), 500),
            Definition = NormalizeOptional(definition, nameof(definition), 4000),
            Example = NormalizeOptional(example, nameof(example), 4000),
            Tags = NormalizeTags(tags),
            CreatedById = teacherId
        };

        _toolsDb.DictionaryWords.Add(word);
        await _toolsDb.SaveChangesAsync(cancellationToken);

        return MapWord(word, courseTitle, null);
    }

    public async Task<DictionaryWordDto> UpdateWordAsync(
        Guid wordId,
        string teacherId,
        Guid courseId,
        string term,
        string translation,
        string? definition,
        string? example,
        IReadOnlyCollection<string>? tags,
        CancellationToken cancellationToken = default)
    {
        var word = await _toolsDb.DictionaryWords
            .FirstOrDefaultAsync(item => item.Id == wordId, cancellationToken);

        if (word is null)
            throw new KeyNotFoundException("Слово не найдено.");

        await EnsureTeacherCourseAccessAsync(word.CourseId, teacherId, cancellationToken);
        var courseTitle = await EnsureTeacherCourseAccessAsync(courseId, teacherId, cancellationToken);

        word.CourseId = courseId;
        word.Term = NormalizeRequired(term, nameof(term), 200);
        word.Translation = NormalizeRequired(translation, nameof(translation), 500);
        word.Definition = NormalizeOptional(definition, nameof(definition), 4000);
        word.Example = NormalizeOptional(example, nameof(example), 4000);
        word.Tags = NormalizeTags(tags);

        await _toolsDb.SaveChangesAsync(cancellationToken);

        return MapWord(word, courseTitle, null);
    }

    public async Task DeleteWordAsync(
        Guid wordId,
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        var word = await _toolsDb.DictionaryWords
            .FirstOrDefaultAsync(item => item.Id == wordId, cancellationToken);

        if (word is null)
            throw new KeyNotFoundException("Слово не найдено.");

        await EnsureTeacherCourseAccessAsync(word.CourseId, teacherId, cancellationToken);

        _toolsDb.DictionaryWords.Remove(word);
        await _toolsDb.SaveChangesAsync(cancellationToken);
    }

    public async Task<DictionaryWordDto> SetStudentProgressAsync(
        Guid wordId,
        string studentId,
        bool isKnown,
        CancellationToken cancellationToken = default)
    {
        var word = await _toolsDb.DictionaryWords
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == wordId, cancellationToken);

        if (word is null)
            throw new KeyNotFoundException("Слово не найдено.");

        await EnsureStudentCourseAccessAsync(word.CourseId, studentId, cancellationToken);

        var progress = await _toolsDb.UserDictionaryProgress
            .FirstOrDefaultAsync(item => item.WordId == wordId && item.UserId == studentId, cancellationToken);

        if (progress is null)
        {
            progress = new UserDictionaryProgress
            {
                WordId = wordId,
                UserId = studentId
            };

            _toolsDb.UserDictionaryProgress.Add(progress);
        }

        ApplyOutcome(progress, isKnown ? DictionaryReviewOutcome.Known : DictionaryReviewOutcome.RepeatLater, DateTime.UtcNow);
        if (!isKnown)
        {
            progress.NextReviewAt = DateTime.UtcNow;
        }

        await _toolsDb.SaveChangesAsync(cancellationToken);

        var courseTitle = await _coursesDb.Courses
            .AsNoTracking()
            .Where(course => course.Id == word.CourseId)
            .Select(course => course.Title)
            .FirstAsync(cancellationToken);

        return MapWord(word, courseTitle, progress);
    }

    public async Task<DictionaryWordDto> ReviewWordAsync(
        Guid wordId,
        string studentId,
        string outcome,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<DictionaryReviewOutcome>(outcome, ignoreCase: true, out var parsedOutcome))
            throw new InvalidOperationException("Неизвестный результат повторения.");

        var word = await _toolsDb.DictionaryWords
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == wordId, cancellationToken);

        if (word is null)
            throw new KeyNotFoundException("Слово не найдено.");

        await EnsureStudentCourseAccessAsync(word.CourseId, studentId, cancellationToken);

        var progress = await _toolsDb.UserDictionaryProgress
            .FirstOrDefaultAsync(item => item.WordId == wordId && item.UserId == studentId, cancellationToken);

        if (progress is null)
        {
            progress = new UserDictionaryProgress
            {
                WordId = wordId,
                UserId = studentId
            };

            _toolsDb.UserDictionaryProgress.Add(progress);
        }

        ApplyOutcome(progress, parsedOutcome, DateTime.UtcNow);

        await _toolsDb.SaveChangesAsync(cancellationToken);

        var courseTitle = await _coursesDb.Courses
            .AsNoTracking()
            .Where(course => course.Id == word.CourseId)
            .Select(course => course.Title)
            .FirstAsync(cancellationToken);

        return MapWord(word, courseTitle, progress);
    }

    private IQueryable<DictionaryWord> BuildWordQuery(
        IReadOnlyCollection<Guid> accessibleCourseIds,
        Guid? courseId,
        string? search)
    {
        var query = _toolsDb.DictionaryWords
            .AsNoTracking()
            .Where(word => accessibleCourseIds.Contains(word.CourseId));

        if (courseId.HasValue)
        {
            query = query.Where(word => word.CourseId == courseId.Value);
        }

        var searchValue = NormalizeOptional(search);
        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            var pattern = $"%{searchValue}%";
            query = query.Where(word =>
                EF.Functions.ILike(word.Term, pattern)
                || EF.Functions.ILike(word.Translation, pattern)
                || (word.Definition != null && EF.Functions.ILike(word.Definition, pattern))
                || (word.Example != null && EF.Functions.ILike(word.Example, pattern))
                || (word.Tags != null && EF.Functions.ILike(word.Tags, pattern)));
        }

        return query;
    }

    private async Task<string> EnsureTeacherCourseAccessAsync(
        Guid courseId,
        string teacherId,
        CancellationToken cancellationToken)
    {
        var course = await _coursesDb.Courses
            .AsNoTracking()
            .Where(item => item.Id == courseId)
            .Select(item => new { item.Title, item.TeacherId })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            throw new KeyNotFoundException("Курс не найден.");

        if (!string.Equals(course.TeacherId, teacherId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Вы не можете изменять словарь этого курса.");

        return course.Title;
    }

    private async Task EnsureStudentCourseAccessAsync(
        Guid courseId,
        string studentId,
        CancellationToken cancellationToken)
    {
        var hasAccess = await _coursesDb.CourseEnrollments
            .AsNoTracking()
            .AnyAsync(
                enrollment =>
                    enrollment.CourseId == courseId
                    && enrollment.StudentId == studentId
                    && enrollment.Status == EnrollmentStatus.Active,
                cancellationToken);

        if (!hasAccess)
            throw new UnauthorizedAccessException("У вас нет доступа к словарю этого курса.");
    }

    private async Task<List<Guid>> GetStudentAccessibleCourseIdsAsync(
        string studentId,
        CancellationToken cancellationToken)
    {
        return await _coursesDb.CourseEnrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.StudentId == studentId
                && enrollment.Status == EnrollmentStatus.Active)
            .Select(enrollment => enrollment.CourseId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, string>> GetCourseTitlesAsync(
        IReadOnlyCollection<Guid> courseIds,
        CancellationToken cancellationToken)
    {
        return await _coursesDb.Courses
            .AsNoTracking()
            .Where(course => courseIds.Contains(course.Id))
            .Select(course => new { course.Id, course.Title })
            .ToDictionaryAsync(course => course.Id, course => course.Title, cancellationToken);
    }

    private async Task<Dictionary<Guid, UserDictionaryProgress>> LoadProgressMapAsync(
        string studentId,
        IEnumerable<Guid> wordIds,
        CancellationToken cancellationToken)
    {
        var ids = wordIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        return await _toolsDb.UserDictionaryProgress
            .AsNoTracking()
            .Where(progress => progress.UserId == studentId && ids.Contains(progress.WordId))
            .ToDictionaryAsync(progress => progress.WordId, progress => progress, cancellationToken);
    }

    private static void ApplyOutcome(UserDictionaryProgress progress, DictionaryReviewOutcome outcome, DateTime reviewedAtUtc)
    {
        progress.ReviewCount += 1;
        progress.LastReviewedAt = reviewedAtUtc;
        progress.LastOutcome = outcome;

        switch (outcome)
        {
            case DictionaryReviewOutcome.Known:
                progress.IsKnown = true;
                progress.NextReviewAt = reviewedAtUtc.Add(GetKnownInterval(progress.ReviewCount));
                break;
            case DictionaryReviewOutcome.Hard:
                progress.IsKnown = false;
                progress.HardCount += 1;
                progress.NextReviewAt = reviewedAtUtc.AddHours(6);
                break;
            case DictionaryReviewOutcome.RepeatLater:
                progress.IsKnown = false;
                progress.RepeatLaterCount += 1;
                progress.NextReviewAt = reviewedAtUtc.AddMinutes(20);
                break;
        }
    }

    private static TimeSpan GetKnownInterval(int reviewCount)
    {
        return reviewCount switch
        {
            <= 1 => TimeSpan.FromDays(1),
            2 => TimeSpan.FromDays(3),
            3 => TimeSpan.FromDays(7),
            _ => TimeSpan.FromDays(14)
        };
    }

    private static DictionaryWordDto MapWord(
        DictionaryWord word,
        string courseTitle,
        UserDictionaryProgress? progress)
    {
        return new DictionaryWordDto
        {
            Id = word.Id,
            CourseId = word.CourseId,
            CourseTitle = courseTitle,
            Term = word.Term,
            Translation = word.Translation,
            Definition = word.Definition,
            Example = word.Example,
            Tags = ParseTags(word.Tags),
            CreatedById = word.CreatedById,
            IsKnown = progress?.IsKnown ?? false,
            ReviewCount = progress?.ReviewCount ?? 0,
            HardCount = progress?.HardCount ?? 0,
            RepeatLaterCount = progress?.RepeatLaterCount ?? 0,
            LastReviewedAt = progress?.LastReviewedAt,
            LastOutcome = progress?.LastOutcome?.ToString(),
            NextReviewAt = progress?.NextReviewAt,
            CreatedAt = word.CreatedAt,
            UpdatedAt = word.UpdatedAt
        };
    }

    private static string NormalizeRequired(string? value, string fieldName, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException($"Поле {fieldName} обязательно.");

        if (normalized.Length > maxLength)
            throw new InvalidOperationException($"Поле {fieldName} превышает допустимую длину.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, string fieldName = "", int? maxLength = null)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (maxLength.HasValue && normalized.Length > maxLength.Value)
            throw new InvalidOperationException($"Поле {fieldName} превышает допустимую длину.");

        return normalized;
    }

    private static string? NormalizeTags(IReadOnlyCollection<string>? tags)
    {
        if (tags is null || tags.Count == 0)
            return null;

        var normalized = tags
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();

        if (normalized.Count == 0)
            return null;

        var serialized = string.Join(", ", normalized);
        return serialized.Length > 1000 ? serialized[..1000] : serialized;
    }

    private static List<string> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return [];

        return tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
