using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Progress.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

public class SubscriptionAllocationReadService : ISubscriptionAllocationReadService
{
    private readonly CoursesDbContext _coursesDbContext;
    private readonly ProgressDbContext _progressDbContext;

    public SubscriptionAllocationReadService(
        CoursesDbContext coursesDbContext,
        ProgressDbContext progressDbContext)
    {
        _coursesDbContext = coursesDbContext;
        _progressDbContext = progressDbContext;
    }

    public async Task<IReadOnlyList<SubscriptionAllocationCandidate>> GetAllocationCandidatesAsync(
        string studentId,
        DateTime? periodStart,
        DateTime? periodEnd,
        CancellationToken cancellationToken = default)
    {
        var courseIds = await _coursesDbContext.CourseEnrollments
            .Where(x => x.StudentId == studentId && x.Status == EnrollmentStatus.Active)
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (courseIds.Count == 0)
            return [];

        var courses = await _coursesDbContext.Courses
            .Where(x => courseIds.Contains(x.Id))
            .Include(x => x.Modules)
                .ThenInclude(x => x.Lessons)
            .ToListAsync(cancellationToken);

        var lessonIds = courses
            .SelectMany(x => x.Modules.Where(m => m.IsPublished)
                .SelectMany(m => m.Lessons.Where(l => l.IsPublished)
                    .Select(l => l.Id)))
            .Distinct()
            .ToList();

        var completedLessonIds = lessonIds.Count == 0
            ? []
            : await _progressDbContext.LessonProgresses
                .Where(x => x.StudentId == studentId
                         && x.IsCompleted
                         && lessonIds.Contains(x.LessonId))
                .Select(x => x.LessonId)
                .ToListAsync(cancellationToken);

        var completedSet = completedLessonIds.ToHashSet();

        return courses
            .Where(x => !string.IsNullOrWhiteSpace(x.TeacherId))
            .Select(course =>
            {
                var publishedLessonIds = course.Modules
                    .Where(m => m.IsPublished)
                    .SelectMany(m => m.Lessons.Where(l => l.IsPublished))
                    .Select(l => l.Id)
                    .Distinct()
                    .ToList();

                var totalLessons = publishedLessonIds.Count;
                var completedLessons = publishedLessonIds.Count(completedSet.Contains);
                var progressPercent = totalLessons == 0
                    ? 0m
                    : Math.Round((decimal)completedLessons / totalLessons, 4, MidpointRounding.AwayFromZero);

                return new SubscriptionAllocationCandidate(
                    course.Id,
                    course.Title,
                    course.TeacherId,
                    course.TeacherName,
                    totalLessons,
                    completedLessons,
                    progressPercent);
            })
            .ToList();
    }
}
