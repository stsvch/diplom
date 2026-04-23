using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.EnrollCourse;

public class EnrollCourseCommandHandler : IRequestHandler<EnrollCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;
    private readonly INotificationDispatcher _notifications;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IAssignmentReadService _assignmentRead;
    private readonly ITestReadService _testRead;
    private readonly IChatAdmin _chatAdmin;

    public EnrollCourseCommandHandler(
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

    public async Task<Result<string>> Handle(EnrollCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync([request.CourseId], cancellationToken);
        if (course == null)
            return Result.Failure<string>("Курс не найден.");

        if (!course.IsPublished || course.IsArchived)
            return Result.Failure<string>("Курс недоступен для записи.");

        var existingEnrollment = await _context.CourseEnrollments
            .FirstOrDefaultAsync(e => e.CourseId == request.CourseId
                                   && e.StudentId == request.StudentId
                                   && e.Status == EnrollmentStatus.Active, cancellationToken);

        if (existingEnrollment != null)
            return Result.Failure<string>("Вы уже записаны на этот курс.");

        var enrollment = new CourseEnrollment
        {
            CourseId = request.CourseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Active
        };

        _context.CourseEnrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        await _notifications.PublishAsync(new NotificationRequest(
            request.StudentId,
            NotificationType.Course,
            "Вы записаны на курс",
            $"Курс «{course.Title}»",
            $"/student/course/{course.Id}"), cancellationToken);

        var upserts = new List<CalendarEventUpsert>();

        var assignments = await _assignmentRead.GetByCourseAsync(request.CourseId, cancellationToken);
        foreach (var a in assignments.Where(x => x.Deadline.HasValue))
        {
            upserts.Add(new CalendarEventUpsert(
                request.StudentId, request.CourseId, a.Title, null,
                DateTime.SpecifyKind(a.Deadline!.Value.Date, DateTimeKind.Utc),
                a.Deadline!.Value.ToString("HH:mm"),
                CalendarEventType.Deadline, "Assignment", a.AssignmentId));
        }

        var tests = await _testRead.GetByCourseAsync(request.CourseId, cancellationToken);
        foreach (var t in tests.Where(x => x.Deadline.HasValue))
        {
            upserts.Add(new CalendarEventUpsert(
                request.StudentId, request.CourseId, t.Title, null,
                DateTime.SpecifyKind(t.Deadline!.Value.Date, DateTimeKind.Utc),
                t.Deadline!.Value.ToString("HH:mm"),
                CalendarEventType.Quiz, "Test", t.TestId));
        }

        if (upserts.Count > 0)
            await _calendar.UpsertManyAsync(upserts, cancellationToken);

        await _chatAdmin.AddParticipantAsync(
            request.CourseId.ToString(),
            request.StudentId,
            request.StudentName,
            cancellationToken);

        return Result.Success<string>("Вы записаны на курс.");
    }
}
