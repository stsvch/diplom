using Assignments.Domain.Enums;
using Assignments.Infrastructure.Persistence;
using Auth.Domain.Entities;
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Models.Reports;
using Grading.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Progress.Infrastructure.Persistence;
using Tests.Domain.Enums;
using Tests.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

public class TeacherCourseReportReadService
{
    private readonly CoursesDbContext _coursesDb;
    private readonly ProgressDbContext _progressDb;
    private readonly GradingDbContext _gradingDb;
    private readonly AssignmentsDbContext _assignmentsDb;
    private readonly TestsDbContext _testsDb;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherCourseReportReadService(
        CoursesDbContext coursesDb,
        ProgressDbContext progressDb,
        GradingDbContext gradingDb,
        AssignmentsDbContext assignmentsDb,
        TestsDbContext testsDb,
        UserManager<ApplicationUser> userManager)
    {
        _coursesDb = coursesDb;
        _progressDb = progressDb;
        _gradingDb = gradingDb;
        _assignmentsDb = assignmentsDb;
        _testsDb = testsDb;
        _userManager = userManager;
    }

    public async Task<TeacherCourseReportDto?> GetAsync(
        string teacherId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await _coursesDb.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId && c.TeacherId == teacherId)
            .Select(c => new
            {
                c.Id,
                c.Title,
                DisciplineName = c.Discipline.Name,
                c.IsPublished,
                c.IsArchived
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return null;

        var studentIds = await _coursesDb.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var lessonIds = await _coursesDb.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .SelectMany(m => m.Lessons.Select(l => l.Id))
            .ToListAsync(cancellationToken);

        var totalLessons = lessonIds.Count;
        var hasStructuredLessons = totalLessons > 0;

        var completedProgressRows = studentIds.Count == 0 || totalLessons == 0
            ? []
            : await _progressDb.LessonProgresses
                .AsNoTracking()
                .Where(p =>
                    p.IsCompleted
                    && studentIds.Contains(p.StudentId)
                    && lessonIds.Contains(p.LessonId))
                .Select(p => new { p.StudentId, p.LessonId })
                .ToListAsync(cancellationToken);

        var completedLessonsByStudent = completedProgressRows
            .GroupBy(p => p.StudentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.LessonId).Distinct().Count());

        var progressRows = studentIds
            .Select(studentId =>
            {
                completedLessonsByStudent.TryGetValue(studentId, out var completedLessons);
                var progressPercent = totalLessons > 0
                    ? (decimal)completedLessons / totalLessons * 100m
                    : 0m;

                return new
                {
                    StudentId = studentId,
                    CompletedLessons = completedLessons,
                    ProgressPercent = progressPercent
                };
            })
            .ToList();

        var gradeRows = studentIds.Count == 0
            ? []
            : await _gradingDb.Grades
                .AsNoTracking()
                .Where(g => g.CourseId == courseId && studentIds.Contains(g.StudentId))
                .Select(g => new
                {
                    g.StudentId,
                    Percent = g.MaxScore > 0 ? g.Score / g.MaxScore * 100m : 0m
                })
                .ToListAsync(cancellationToken);

        var averageGradeByStudent = gradeRows
            .GroupBy(g => g.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.Any() ? Math.Round(g.Average(x => x.Percent), 1) : 0m);

        var assignments = await _assignmentsDb.Assignments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Deadline
            })
            .ToListAsync(cancellationToken);

        var tests = await _testsDb.Tests
            .AsNoTracking()
            .Where(t => t.CourseId == courseId)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Deadline
            })
            .ToListAsync(cancellationToken);

        var assignmentIds = assignments.Select(a => a.Id).ToList();
        var testIds = tests.Select(t => t.Id).ToList();

        var assignmentSubmissions = assignmentIds.Count == 0 || studentIds.Count == 0
            ? []
            : await _assignmentsDb.AssignmentSubmissions
                .AsNoTracking()
                .Where(s => assignmentIds.Contains(s.AssignmentId) && studentIds.Contains(s.StudentId))
                .Select(s => new AssignmentSubmissionSnapshot(
                    s.AssignmentId,
                    s.StudentId,
                    s.AttemptNumber,
                    s.Status,
                    s.SubmittedAt))
                .ToListAsync(cancellationToken);

        var testAttempts = testIds.Count == 0 || studentIds.Count == 0
            ? []
            : await _testsDb.TestAttempts
                .AsNoTracking()
                .Where(a => testIds.Contains(a.TestId) && studentIds.Contains(a.StudentId))
                .Select(a => new TestAttemptSnapshot(
                    a.TestId,
                    a.StudentId,
                    a.AttemptNumber,
                    a.Status,
                    a.CompletedAt ?? a.StartedAt))
                .ToListAsync(cancellationToken);

        var latestAssignmentByStudent = assignmentSubmissions
            .GroupBy(s => (s.AssignmentId, s.StudentId))
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.AttemptNumber)
                    .ThenByDescending(x => x.SubmittedAt)
                    .First());

        var latestTestByStudent = testAttempts
            .GroupBy(a => (a.TestId, a.StudentId))
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.AttemptNumber)
                    .ThenByDescending(x => x.ActivityAt)
                    .First());

        var pendingAssignmentBySource = assignmentSubmissions
            .Where(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview)
            .GroupBy(s => s.AssignmentId)
            .ToDictionary(g => g.Key, g => g.Count());

        var pendingTestBySource = testAttempts
            .Where(a => a.Status == AttemptStatus.NeedsReview)
            .GroupBy(a => a.TestId)
            .ToDictionary(g => g.Key, g => g.Count());

        var pendingAssignmentByStudent = assignmentSubmissions
            .Where(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview)
            .GroupBy(s => s.StudentId)
            .ToDictionary(g => g.Key, g => g.Count());

        var pendingTestByStudent = testAttempts
            .Where(a => a.Status == AttemptStatus.NeedsReview)
            .GroupBy(a => a.StudentId)
            .ToDictionary(g => g.Key, g => g.Count());

        var now = DateTime.UtcNow;
        var upcomingDeadlineThreshold = now.AddDays(7);

        var deadlineItems = new List<TeacherCourseDeadlineItemDto>();
        var overdueAssignmentsCount = 0;
        var overdueTestsCount = 0;

        foreach (var assignment in assignments.Where(a => a.Deadline.HasValue))
        {
            var affectedStudentsCount = studentIds.Count(studentId =>
                IsAssignmentOutstanding(latestAssignmentByStudent, assignment.Id, studentId));
            var pendingReviewsCount = pendingAssignmentBySource.TryGetValue(assignment.Id, out var pending) ? pending : 0;
            var isOverdue = assignment.Deadline!.Value < now && affectedStudentsCount > 0;

            if (isOverdue)
            {
                overdueAssignmentsCount += affectedStudentsCount;
            }

            if (isOverdue || (assignment.Deadline <= upcomingDeadlineThreshold && affectedStudentsCount > 0) || pendingReviewsCount > 0)
            {
                deadlineItems.Add(new TeacherCourseDeadlineItemDto
                {
                    Kind = "Assignment",
                    SourceId = assignment.Id,
                    Title = assignment.Title,
                    Deadline = assignment.Deadline.Value,
                    IsOverdue = isOverdue,
                    AffectedStudentsCount = affectedStudentsCount,
                    PendingReviewsCount = pendingReviewsCount
                });
            }
        }

        foreach (var test in tests.Where(t => t.Deadline.HasValue))
        {
            var affectedStudentsCount = studentIds.Count(studentId =>
                IsTestOutstanding(latestTestByStudent, test.Id, studentId));
            var pendingReviewsCount = pendingTestBySource.TryGetValue(test.Id, out var pending) ? pending : 0;
            var isOverdue = test.Deadline!.Value < now && affectedStudentsCount > 0;

            if (isOverdue)
            {
                overdueTestsCount += affectedStudentsCount;
            }

            if (isOverdue || (test.Deadline <= upcomingDeadlineThreshold && affectedStudentsCount > 0) || pendingReviewsCount > 0)
            {
                deadlineItems.Add(new TeacherCourseDeadlineItemDto
                {
                    Kind = "Test",
                    SourceId = test.Id,
                    Title = test.Title,
                    Deadline = test.Deadline.Value,
                    IsOverdue = isOverdue,
                    AffectedStudentsCount = affectedStudentsCount,
                    PendingReviewsCount = pendingReviewsCount
                });
            }
        }

        var studentNames = studentIds.Count == 0
            ? new Dictionary<string, string>()
            : await _userManager.Users
                .Where(u => studentIds.Contains(u.Id))
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

        var atRiskStudents = progressRows
            .Select(row =>
            {
                averageGradeByStudent.TryGetValue(row.StudentId, out var averageGradePercent);
                var overdueAssignments = assignments.Count(a =>
                    a.Deadline.HasValue
                    && a.Deadline.Value < now
                    && IsAssignmentOutstanding(latestAssignmentByStudent, a.Id, row.StudentId));
                var overdueTests = tests.Count(t =>
                    t.Deadline.HasValue
                    && t.Deadline.Value < now
                    && IsTestOutstanding(latestTestByStudent, t.Id, row.StudentId));
                var pendingReviews = (pendingAssignmentByStudent.TryGetValue(row.StudentId, out var assignmentPending) ? assignmentPending : 0)
                    + (pendingTestByStudent.TryGetValue(row.StudentId, out var testPending) ? testPending : 0);

                return new
                {
                    Student = new TeacherCourseRiskStudentDto
                    {
                        StudentId = row.StudentId,
                        StudentName = studentNames.TryGetValue(row.StudentId, out var name) ? name : row.StudentId,
                        CompletedLessons = row.CompletedLessons,
                        TotalLessons = totalLessons,
                        ProgressPercent = Math.Round(row.ProgressPercent, 1),
                        AverageGradePercent = averageGradeByStudent.TryGetValue(row.StudentId, out var grade)
                            ? grade
                            : null,
                        OverdueAssignmentsCount = overdueAssignments,
                        OverdueTestsCount = overdueTests,
                        PendingReviewCount = pendingReviews
                    },
                    RiskScore = overdueAssignments * 100
                        + overdueTests * 100
                        + (hasStructuredLessons ? 100 - row.ProgressPercent : 0m)
                        + (averageGradeByStudent.TryGetValue(row.StudentId, out var gradeValue) ? 100 - gradeValue : 15m)
                };
            })
            .Where(x =>
                x.Student.OverdueAssignmentsCount > 0
                || x.Student.OverdueTestsCount > 0
                || (hasStructuredLessons && x.Student.ProgressPercent < 60)
                || (x.Student.AverageGradePercent.HasValue && x.Student.AverageGradePercent.Value < 70))
            .OrderByDescending(x => x.RiskScore)
            .ThenBy(x => x.Student.StudentName)
            .Take(8)
            .Select(x => x.Student)
            .ToList();

        var overdueStudentsCount = progressRows.Count(row =>
            assignments.Any(a =>
                a.Deadline.HasValue
                && a.Deadline.Value < now
                && IsAssignmentOutstanding(latestAssignmentByStudent, a.Id, row.StudentId))
            || tests.Any(t =>
                t.Deadline.HasValue
                && t.Deadline.Value < now
                && IsTestOutstanding(latestTestByStudent, t.Id, row.StudentId)));

        var summary = new TeacherCourseReportSummaryDto
        {
            CourseId = course.Id,
            Title = course.Title,
            DisciplineName = course.DisciplineName,
            IsPublished = course.IsPublished,
            IsArchived = course.IsArchived,
            ActiveStudents = studentIds.Count,
            TotalLessons = totalLessons,
            AverageProgressPercent = progressRows.Count > 0
                ? Math.Round(progressRows.Average(x => x.ProgressPercent), 1)
                : 0m,
            CompletionRatePercent = progressRows.Count > 0 && totalLessons > 0
                ? Math.Round(
                    progressRows.Count(x => x.CompletedLessons >= totalLessons) / (decimal)progressRows.Count * 100m,
                    1)
                : 0m,
            AverageGradePercent = gradeRows.Count > 0
                ? Math.Round(gradeRows.Average(x => x.Percent), 1)
                : 0m,
            PendingReviewsCount = pendingAssignmentBySource.Values.Sum() + pendingTestBySource.Values.Sum(),
            OverdueStudentsCount = overdueStudentsCount,
            OverdueAssignmentsCount = overdueAssignmentsCount,
            OverdueTestsCount = overdueTestsCount,
            UpcomingDeadlinesCount = deadlineItems.Count(x => !x.IsOverdue && x.Deadline <= upcomingDeadlineThreshold)
        };

        return new TeacherCourseReportDto
        {
            Summary = summary,
            GradeDistribution = BuildGradeDistribution(gradeRows.Select(x => x.Percent).ToList()),
            AtRiskStudents = atRiskStudents,
            Deadlines = deadlineItems
                .OrderByDescending(x => x.IsOverdue)
                .ThenBy(x => x.Deadline)
                .Take(8)
                .ToList()
        };
    }

    private static List<TeacherCourseGradeBucketDto> BuildGradeDistribution(IReadOnlyCollection<decimal> grades)
    {
        var total = grades.Count;
        var buckets = new[]
        {
            new { Label = "90-100", Count = grades.Count(x => x >= 90) },
            new { Label = "75-89", Count = grades.Count(x => x >= 75 && x < 90) },
            new { Label = "60-74", Count = grades.Count(x => x >= 60 && x < 75) },
            new { Label = "< 60", Count = grades.Count(x => x < 60) },
        };

        return buckets
            .Select(bucket => new TeacherCourseGradeBucketDto
            {
                Label = bucket.Label,
                Count = bucket.Count,
                SharePercent = total > 0
                    ? Math.Round(bucket.Count / (decimal)total * 100m, 1)
                    : 0m
            })
            .ToList();
    }

    private static bool IsAssignmentOutstanding(
        IReadOnlyDictionary<(Guid AssignmentId, string StudentId), AssignmentSubmissionSnapshot> latestAssignmentByStudent,
        Guid assignmentId,
        string studentId)
    {
        if (!latestAssignmentByStudent.TryGetValue((assignmentId, studentId), out var latestSubmission))
            return true;

        return latestSubmission.Status == SubmissionStatus.ReturnedForRevision;
    }

    private static bool IsTestOutstanding(
        IReadOnlyDictionary<(Guid TestId, string StudentId), TestAttemptSnapshot> latestTestByStudent,
        Guid testId,
        string studentId)
    {
        if (!latestTestByStudent.TryGetValue((testId, studentId), out var latestAttempt))
            return true;

        return latestAttempt.Status == AttemptStatus.InProgress;
    }

    private sealed record AssignmentSubmissionSnapshot(
        Guid AssignmentId,
        string StudentId,
        int AttemptNumber,
        SubmissionStatus Status,
        DateTime SubmittedAt);

    private sealed record TestAttemptSnapshot(
        Guid TestId,
        string StudentId,
        int AttemptNumber,
        AttemptStatus Status,
        DateTime ActivityAt);
}
