using Assignments.Domain.Enums;
using Assignments.Infrastructure.Persistence;
using Auth.Domain.Entities;
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Models.Reports;
using Grading.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Interfaces;
using Progress.Infrastructure.Persistence;
using Scheduling.Domain.Enums;
using Scheduling.Infrastructure.Persistence;
using Tests.Domain.Enums;
using Tests.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

public class TeacherDashboardReadService
{
    private readonly CoursesDbContext _coursesDb;
    private readonly ProgressDbContext _progressDb;
    private readonly GradingDbContext _gradingDb;
    private readonly AssignmentsDbContext _assignmentsDb;
    private readonly TestsDbContext _testsDb;
    private readonly SchedulingDbContext _schedulingDb;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPaymentsService _paymentsService;

    public TeacherDashboardReadService(
        CoursesDbContext coursesDb,
        ProgressDbContext progressDb,
        GradingDbContext gradingDb,
        AssignmentsDbContext assignmentsDb,
        TestsDbContext testsDb,
        SchedulingDbContext schedulingDb,
        UserManager<ApplicationUser> userManager,
        IPaymentsService paymentsService)
    {
        _coursesDb = coursesDb;
        _progressDb = progressDb;
        _gradingDb = gradingDb;
        _assignmentsDb = assignmentsDb;
        _testsDb = testsDb;
        _schedulingDb = schedulingDb;
        _userManager = userManager;
        _paymentsService = paymentsService;
    }

    public async Task<TeacherDashboardDto> GetAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        var courses = await _coursesDb.Courses
            .AsNoTracking()
            .Where(c => c.TeacherId == teacherId)
            .Select(c => new
            {
                c.Id,
                c.Title,
                DisciplineName = c.Discipline.Name,
                c.IsPublished,
                c.IsArchived
            })
            .ToListAsync(cancellationToken);

        var courseIds = courses.Select(c => c.Id).ToList();

        var activeEnrollments = courseIds.Count == 0
            ? []
            : await _coursesDb.CourseEnrollments
                .AsNoTracking()
                .Where(e => courseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.Active)
                .Select(e => new { e.CourseId, e.StudentId })
                .ToListAsync(cancellationToken);

        var lessonRows = courseIds.Count == 0
            ? []
            : await _coursesDb.CourseModules
                .AsNoTracking()
                .Where(m => courseIds.Contains(m.CourseId))
                .SelectMany(m => m.Lessons.Select(l => new { m.CourseId, LessonId = l.Id }))
                .ToListAsync(cancellationToken);

        var lessonIds = lessonRows.Select(x => x.LessonId).Distinct().ToList();
        var activeStudentIds = activeEnrollments.Select(x => x.StudentId).Distinct().ToList();

        var completedProgressRows = lessonIds.Count == 0 || activeStudentIds.Count == 0
            ? []
            : await _progressDb.LessonProgresses
                .AsNoTracking()
                .Where(p =>
                    p.IsCompleted
                    && activeStudentIds.Contains(p.StudentId)
                    && lessonIds.Contains(p.LessonId))
                .Select(p => new { p.StudentId, p.LessonId })
                .ToListAsync(cancellationToken);

        var lessonCourseById = lessonRows
            .GroupBy(x => x.LessonId)
            .ToDictionary(g => g.Key, g => g.First().CourseId);

        var completedByStudentCourse = completedProgressRows
            .Select(row => new
            {
                row.StudentId,
                CourseId = lessonCourseById.TryGetValue(row.LessonId, out var courseId) ? courseId : Guid.Empty
            })
            .Where(x => x.CourseId != Guid.Empty)
            .GroupBy(x => new { x.StudentId, x.CourseId })
            .ToDictionary(g => (g.Key.StudentId, g.Key.CourseId), g => g.Count());

        var totalLessonsByCourse = lessonRows
            .GroupBy(x => x.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());

        var enrollmentProgressRows = activeEnrollments
            .Select(enrollment =>
            {
                totalLessonsByCourse.TryGetValue(enrollment.CourseId, out var totalLessons);
                completedByStudentCourse.TryGetValue((enrollment.StudentId, enrollment.CourseId), out var completedLessons);

                var progressPercent = totalLessons > 0
                    ? (decimal)completedLessons / totalLessons * 100m
                    : 0m;

                return new
                {
                    enrollment.CourseId,
                    enrollment.StudentId,
                    ProgressPercent = progressPercent
                };
            })
            .ToList();

        var averageProgressByCourse = enrollmentProgressRows
            .GroupBy(x => x.CourseId)
            .ToDictionary(g => g.Key, g => g.Any() ? Math.Round(g.Average(x => x.ProgressPercent), 1) : 0m);

        var activeStudentsByCourse = activeEnrollments
            .GroupBy(x => x.CourseId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.StudentId).Distinct().Count());

        var gradeRows = courseIds.Count == 0
            ? []
            : await _gradingDb.Grades
                .AsNoTracking()
                .Where(g => courseIds.Contains(g.CourseId))
                .Select(g => new
                {
                    g.CourseId,
                    Percent = g.MaxScore > 0 ? g.Score / g.MaxScore * 100m : 0m
                })
                .ToListAsync(cancellationToken);

        var averageGradeByCourse = gradeRows
            .GroupBy(x => x.CourseId)
            .ToDictionary(g => g.Key, g => g.Any() ? Math.Round(g.Average(x => x.Percent), 1) : 0m);

        var pendingAssignmentRows = await _assignmentsDb.AssignmentSubmissions
            .AsNoTracking()
            .Where(s =>
                s.Assignment.CreatedById == teacherId
                && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview))
            .Select(s => new
            {
                Kind = "Assignment",
                SourceId = s.AssignmentId,
                ReviewId = s.Id,
                s.StudentId,
                s.SubmittedAt,
                CourseId = s.Assignment.CourseId,
                Title = s.Assignment.Title
            })
            .ToListAsync(cancellationToken);

        var pendingTestRows = await _testsDb.TestAttempts
            .AsNoTracking()
            .Where(a => a.Test.CreatedById == teacherId && a.Status == AttemptStatus.NeedsReview)
            .Select(a => new
            {
                Kind = "Test",
                SourceId = a.TestId,
                ReviewId = a.Id,
                a.StudentId,
                SubmittedAt = a.CompletedAt ?? a.StartedAt,
                CourseId = a.Test.CourseId,
                Title = a.Test.Title
            })
            .ToListAsync(cancellationToken);

        var pendingReviewRows = pendingAssignmentRows
            .Concat(pendingTestRows)
            .OrderBy(x => x.SubmittedAt)
            .ToList();

        var reviewStudentIds = pendingReviewRows
            .Select(x => x.StudentId)
            .Distinct()
            .ToList();

        var reviewStudentNames = reviewStudentIds.Count == 0
            ? new Dictionary<string, string>()
            : await _userManager.Users
                .Where(u => reviewStudentIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.UserName })
                .ToDictionaryAsync(
                    u => u.Id,
                    u =>
                    {
                        var fullName = $"{u.FirstName} {u.LastName}".Trim();
                        if (!string.IsNullOrWhiteSpace(fullName))
                            return fullName;

                        return u.Email ?? u.UserName ?? u.Id;
                    },
                    cancellationToken);

        var courseNames = courses.ToDictionary(c => c.Id, c => c.Title);
        var pendingReviewsByCourse = pendingReviewRows
            .Where(x => x.CourseId.HasValue)
            .GroupBy(x => x.CourseId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var pendingReviews = pendingReviewRows
            .Take(6)
            .Select(item => new TeacherDashboardReviewItemDto
            {
                Kind = item.Kind,
                SourceId = item.SourceId,
                ReviewId = item.ReviewId,
                CourseId = item.CourseId,
                CourseName = item.CourseId.HasValue && courseNames.TryGetValue(item.CourseId.Value, out var courseName)
                    ? courseName
                    : null,
                Title = item.Title,
                StudentId = item.StudentId,
                StudentName = reviewStudentNames.TryGetValue(item.StudentId, out var studentName)
                    ? studentName
                    : item.StudentId,
                SubmittedAt = item.SubmittedAt
            })
            .ToList();

        var now = DateTime.UtcNow;
        var upcomingSessionsData = await _schedulingDb.ScheduleSlots
            .AsNoTracking()
            .Where(slot =>
                slot.TeacherId == teacherId
                && slot.StartTime >= now
                && slot.Status != SlotStatus.Cancelled)
            .Select(slot => new
            {
                slot.Id,
                slot.CourseId,
                slot.Title,
                slot.CourseName,
                slot.StartTime,
                slot.EndTime,
                slot.Status,
                slot.IsGroupSession,
                slot.MaxStudents,
                BookingsCount = slot.Bookings.Count(b => b.Status == BookingStatus.Booked)
            })
            .OrderBy(slot => slot.StartTime)
            .ToListAsync(cancellationToken);

        var upcomingSessions = upcomingSessionsData
            .Take(6)
            .Select(slot => new TeacherDashboardSessionDto
            {
                SlotId = slot.Id,
                CourseId = slot.CourseId,
                Title = slot.Title,
                CourseName = slot.CourseName,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Status = slot.Status.ToString(),
                IsGroupSession = slot.IsGroupSession,
                BookingsCount = slot.BookingsCount,
                MaxStudents = slot.MaxStudents
            })
            .ToList();

        var payoutSummary = await _paymentsService.GetTeacherSettlementSummaryAsync(teacherId, cancellationToken);

        var courseCards = courses
            .Select(course => new TeacherDashboardCourseDto
            {
                CourseId = course.Id,
                Title = course.Title,
                DisciplineName = course.DisciplineName,
                IsPublished = course.IsPublished,
                IsArchived = course.IsArchived,
                ActiveStudents = activeStudentsByCourse.TryGetValue(course.Id, out var studentsCount) ? studentsCount : 0,
                PendingReviewsCount = pendingReviewsByCourse.TryGetValue(course.Id, out var reviewsCount) ? reviewsCount : 0,
                AverageStudentProgressPercent = averageProgressByCourse.TryGetValue(course.Id, out var progressPercent) ? progressPercent : 0m,
                AverageGradePercent = averageGradeByCourse.TryGetValue(course.Id, out var gradePercent) ? gradePercent : 0m
            })
            .OrderByDescending(c => c.ActiveStudents)
            .ThenByDescending(c => c.PendingReviewsCount)
            .ThenBy(c => c.Title)
            .Take(6)
            .ToList();

        var averageStudentProgressPercent = enrollmentProgressRows.Count > 0
            ? Math.Round(enrollmentProgressRows.Average(x => x.ProgressPercent), 1)
            : 0m;
        var averageGradePercent = gradeRows.Count > 0
            ? Math.Round(gradeRows.Average(x => x.Percent), 1)
            : 0m;

        return new TeacherDashboardDto
        {
            Summary = new TeacherDashboardSummaryDto
            {
                TotalCourses = courses.Count,
                PublishedCourses = courses.Count(c => c.IsPublished && !c.IsArchived),
                ActiveStudents = activeEnrollments.Select(x => x.StudentId).Distinct().Count(),
                PendingReviewsCount = pendingReviewRows.Count,
                AverageStudentProgressPercent = averageStudentProgressPercent,
                AverageGradePercent = averageGradePercent,
                UpcomingSessionsCount = upcomingSessionsData.Count
            },
            Earnings = new TeacherDashboardEarningsDto
            {
                ReadyForPayoutAmount = payoutSummary.ReadyForPayoutNetAmount,
                InPayoutAmount = payoutSummary.InPayoutNetAmount,
                PaidOutAmount = payoutSummary.PaidOutNetAmount,
                Currency = payoutSummary.Currency
            },
            Courses = courseCards,
            PendingReviews = pendingReviews,
            UpcomingSessions = upcomingSessions
        };
    }
}
