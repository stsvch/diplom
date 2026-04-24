using Calendar.Application.Calendar.Queries.GetUpcomingEvents;
using Calendar.Infrastructure.Persistence;
using Courses.Domain.Enums;
using EduPlatform.Host.Models.Reports;
using Grading.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Progress.Infrastructure.Persistence;
using Courses.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

public class StudentDashboardReadService
{
    private readonly CoursesDbContext _coursesDb;
    private readonly ProgressDbContext _progressDb;
    private readonly GradingDbContext _gradingDb;
    private readonly CalendarDbContext _calendarDb;
    private readonly IMediator _mediator;

    public StudentDashboardReadService(
        CoursesDbContext coursesDb,
        ProgressDbContext progressDb,
        GradingDbContext gradingDb,
        CalendarDbContext calendarDb,
        IMediator mediator)
    {
        _coursesDb = coursesDb;
        _progressDb = progressDb;
        _gradingDb = gradingDb;
        _calendarDb = calendarDb;
        _mediator = mediator;
    }

    public async Task<StudentDashboardDto> GetAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _coursesDb.CourseEnrollments
            .AsNoTracking()
            .Where(e =>
                e.StudentId == studentId
                && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed))
            .Select(e => new
            {
                e.CourseId,
                e.Status,
                CourseTitle = e.Course.Title,
                e.Course.TeacherName,
                DisciplineName = e.Course.Discipline.Name,
                e.Course.ImageUrl,
                e.Course.Deadline
            })
            .OrderBy(e => e.CourseTitle)
            .ToListAsync(cancellationToken);

        var enrolledCourseIds = enrollments
            .Select(e => e.CourseId)
            .Distinct()
            .ToList();

        var lessonRows = enrolledCourseIds.Count == 0
            ? new List<(Guid CourseId, Guid LessonId)>()
            : await _coursesDb.CourseModules
                .AsNoTracking()
                .Where(m => enrolledCourseIds.Contains(m.CourseId))
                .SelectMany(m => m.Lessons.Select(l => new ValueTuple<Guid, Guid>(m.CourseId, l.Id)))
                .ToListAsync(cancellationToken);

        var lessonIds = lessonRows
            .Select(x => x.Item2)
            .Distinct()
            .ToList();

        var completedLessonIds = lessonIds.Count == 0
            ? new HashSet<Guid>()
            : await _progressDb.LessonProgresses
                .AsNoTracking()
                .Where(p => p.StudentId == studentId && p.IsCompleted && lessonIds.Contains(p.LessonId))
                .Select(p => p.LessonId)
                .ToHashSetAsync(cancellationToken);

        var completedLessonsByCourse = lessonRows
            .GroupBy(x => x.Item1)
            .ToDictionary(
                g => g.Key,
                g => g.Count(x => completedLessonIds.Contains(x.Item2)));

        var totalLessonsByCourse = lessonRows
            .GroupBy(x => x.Item1)
            .ToDictionary(g => g.Key, g => g.Count());

        var courses = enrollments
            .Select(enrollment =>
            {
                totalLessonsByCourse.TryGetValue(enrollment.CourseId, out var totalLessons);
                completedLessonsByCourse.TryGetValue(enrollment.CourseId, out var completedLessons);

                var progressPercent = totalLessons > 0
                    ? Math.Round((decimal)completedLessons / totalLessons * 100m, 1)
                    : 0m;
                var isCompleted = enrollment.Status == EnrollmentStatus.Completed
                    || (totalLessons > 0 && completedLessons >= totalLessons);

                return new StudentDashboardCourseDto
                {
                    CourseId = enrollment.CourseId,
                    Title = enrollment.CourseTitle,
                    TeacherName = enrollment.TeacherName,
                    DisciplineName = enrollment.DisciplineName,
                    ImageUrl = enrollment.ImageUrl,
                    Deadline = enrollment.Deadline,
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons,
                    ProgressPercent = progressPercent,
                    IsCompleted = isCompleted
                };
            })
            .OrderByDescending(c => c.ProgressPercent)
            .ThenBy(c => c.Title)
            .ToList();

        var gradeRows = await _gradingDb.Grades
            .AsNoTracking()
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.GradedAt)
            .ToListAsync(cancellationToken);

        var upcomingResult = await _mediator.Send(new GetUpcomingEventsQuery(studentId, 6), cancellationToken);
        var upcomingEvents = upcomingResult.IsSuccess && upcomingResult.Value is not null
            ? upcomingResult.Value
            : [];
        var now = DateTime.UtcNow.Date;
        var totalUpcomingEventsCount = await _calendarDb.CalendarEvents
            .AsNoTracking()
            .CountAsync(
                e => (e.UserId == null || e.UserId == studentId) && e.EventDate >= now,
                cancellationToken);

        var knownCourseNames = courses
            .ToDictionary(c => c.CourseId, c => c.Title);

        var missingCourseIds = gradeRows
            .Select(g => g.CourseId)
            .Concat(upcomingEvents.Where(e => e.CourseId.HasValue).Select(e => e.CourseId!.Value))
            .Where(id => !knownCourseNames.ContainsKey(id))
            .Distinct()
            .ToList();

        if (missingCourseIds.Count > 0)
        {
            var extraNames = await _coursesDb.Courses
                .AsNoTracking()
                .Where(c => missingCourseIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Title })
                .ToListAsync(cancellationToken);

            foreach (var course in extraNames)
            {
                knownCourseNames[course.Id] = course.Title;
            }
        }

        var recentGradeRows = gradeRows
            .Take(6)
            .ToList();

        var recentGrades = recentGradeRows
            .Select(grade =>
            {
                var percent = grade.MaxScore > 0
                    ? Math.Round(grade.Score / grade.MaxScore * 100m, 1)
                    : 0m;

                return new StudentDashboardGradeDto
                {
                    Id = grade.Id,
                    CourseId = grade.CourseId,
                    CourseName = knownCourseNames.TryGetValue(grade.CourseId, out var courseName)
                        ? courseName
                        : $"Курс {grade.CourseId.ToString("N")[..8]}",
                    Title = grade.Title,
                    SourceType = grade.SourceType.ToString(),
                    Score = grade.Score,
                    MaxScore = grade.MaxScore,
                    Percent = percent,
                    GradedAt = grade.GradedAt
                };
            })
            .ToList();

        var upcoming = upcomingEvents
            .Select(item => new StudentDashboardUpcomingItemDto
            {
                Id = item.Id,
                CourseId = item.CourseId,
                CourseName = item.CourseId.HasValue && knownCourseNames.TryGetValue(item.CourseId.Value, out var courseName)
                    ? courseName
                    : null,
                Title = item.Title,
                Description = item.Description,
                EventDate = item.EventDate,
                EventTime = item.EventTime,
                Type = item.Type.ToString(),
                SourceType = item.SourceType,
                SourceId = item.SourceId,
                Status = item.Status?.ToString()
            })
            .ToList();

        var totalLessons = courses.Sum(c => c.TotalLessons);
        var completedLessons = courses.Sum(c => c.CompletedLessons);
        var completedCourses = courses.Count(c => c.IsCompleted);
        var averageGradePercent = recentGradeRows.Count > 0
            ? Math.Round(recentGradeRows.Average(g => g.MaxScore > 0 ? g.Score / g.MaxScore * 100m : 0m), 1)
            : 0m;

        return new StudentDashboardDto
        {
            Summary = new StudentDashboardSummaryDto
            {
                EnrolledCourses = courses.Count,
                CompletedCourses = completedCourses,
                ActiveCourses = Math.Max(0, courses.Count - completedCourses),
                CompletedLessons = completedLessons,
                TotalLessons = totalLessons,
                OverallProgressPercent = totalLessons > 0
                    ? Math.Round((decimal)completedLessons / totalLessons * 100m, 1)
                    : 0m,
                AverageGradePercent = averageGradePercent,
                UpcomingEventsCount = totalUpcomingEventsCount
            },
            Courses = courses,
            RecentGrades = recentGrades,
            Upcoming = upcoming
        };
    }
}
