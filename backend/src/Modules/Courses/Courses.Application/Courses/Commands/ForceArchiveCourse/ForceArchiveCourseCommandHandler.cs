using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.ForceArchiveCourse;

public class ForceArchiveCourseCommandHandler : IRequestHandler<ForceArchiveCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;
    private readonly ICalendarEventPublisher _calendar;
    private readonly INotificationDispatcher _notifications;
    private readonly IChatAdmin _chatAdmin;

    public ForceArchiveCourseCommandHandler(
        ICoursesDbContext context,
        ICalendarEventPublisher calendar,
        INotificationDispatcher notifications,
        IChatAdmin chatAdmin)
    {
        _context = context;
        _calendar = calendar;
        _notifications = notifications;
        _chatAdmin = chatAdmin;
    }

    public async Task<Result<string>> Handle(ForceArchiveCourseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure<string>("Необходимо указать причину архивации.");

        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (course == null)
            return Result.Failure<string>("Курс не найден.");

        course.IsArchived = true;
        course.IsPublished = false;
        course.ArchiveReason = request.Reason;
        course.ArchivedBy = request.AdminId;

        await _context.SaveChangesAsync(cancellationToken);

        await _calendar.DeleteByCourseAsync(request.Id, cancellationToken);
        await _chatAdmin.ArchiveCourseChatAsync(request.Id.ToString(), cancellationToken);

        var activeStudentIds = course.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentId)
            .ToList();

        var recipients = activeStudentIds.Append(course.TeacherId).Distinct().ToList();
        if (recipients.Count > 0)
        {
            var notifications = recipients.Select(uid => new NotificationRequest(
                uid, NotificationType.Course,
                "Курс архивирован администратором",
                $"Курс «{course.Title}» был архивирован. Причина: {request.Reason}",
                "/")).ToList();
            await _notifications.PublishManyAsync(notifications, cancellationToken);
        }

        return Result.Success("Курс архивирован администратором.");
    }
}
