using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Courses.Infrastructure.Services;

public class CourseAccessProvisioningService : ICourseAccessProvisioningService
{
    private readonly ICoursesDbContext _context;
    private readonly INotificationDispatcher _notifications;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IAssignmentReadService _assignmentRead;
    private readonly ITestReadService _testRead;
    private readonly IChatAdmin _chatAdmin;

    public CourseAccessProvisioningService(
        ICoursesDbContext context,
        INotificationDispatcher notifications,
        ICalendarEventPublisher calendar,
        IAssignmentReadService assignmentRead,
        ITestReadService testRead,
        IChatAdmin chatAdmin)
    {
        _context = context;
        _notifications = notifications;
        _calendar = calendar;
        _assignmentRead = assignmentRead;
        _testRead = testRead;
        _chatAdmin = chatAdmin;
    }

    public async Task<Result<string>> GrantAccessAsync(
        Guid courseId,
        string studentId,
        string studentName,
        CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses.FindAsync([courseId], cancellationToken);
        if (course == null)
            return Result.Failure<string>("Курс не найден.");

        if (!course.IsPublished || course.IsArchived)
            return Result.Failure<string>("Курс недоступен для записи.");

        var existingEnrollment = await _context.CourseEnrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId
                                   && e.StudentId == studentId
                                   && e.Status == EnrollmentStatus.Active, cancellationToken);

        if (existingEnrollment != null)
            return Result.Success<string>("Доступ к курсу уже активирован.");

        var enrollment = new CourseEnrollment
        {
            CourseId = courseId,
            StudentId = studentId,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Active
        };

        _context.CourseEnrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        await _notifications.PublishAsync(new NotificationRequest(
            studentId,
            NotificationType.Course,
            "Вы записаны на курс",
            $"Курс «{course.Title}»",
            $"/student/course/{course.Id}"), cancellationToken);

        var upserts = new List<CalendarEventUpsert>();

        var assignments = await _assignmentRead.GetByCourseAsync(courseId, cancellationToken);
        foreach (var assignment in assignments.Where(x => x.Deadline.HasValue))
        {
            upserts.Add(new CalendarEventUpsert(
                studentId,
                courseId,
                assignment.Title,
                null,
                DateTime.SpecifyKind(assignment.Deadline!.Value.Date, DateTimeKind.Utc),
                assignment.Deadline!.Value.ToString("HH:mm"),
                CalendarEventType.Deadline,
                "Assignment",
                assignment.AssignmentId));
        }

        var tests = await _testRead.GetByCourseAsync(courseId, cancellationToken);
        foreach (var test in tests.Where(x => x.Deadline.HasValue))
        {
            upserts.Add(new CalendarEventUpsert(
                studentId,
                courseId,
                test.Title,
                null,
                DateTime.SpecifyKind(test.Deadline!.Value.Date, DateTimeKind.Utc),
                test.Deadline!.Value.ToString("HH:mm"),
                CalendarEventType.Quiz,
                "Test",
                test.TestId));
        }

        if (upserts.Count > 0)
            await _calendar.UpsertManyAsync(upserts, cancellationToken);

        await _chatAdmin.AddParticipantAsync(
            courseId.ToString(),
            studentId,
            studentName,
            cancellationToken);

        return Result.Success<string>("Вы записаны на курс.");
    }
}
