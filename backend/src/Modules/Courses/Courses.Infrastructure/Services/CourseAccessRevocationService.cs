using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace Courses.Infrastructure.Services;

public class CourseAccessRevocationService : ICourseAccessRevocationService
{
    private readonly ICoursesDbContext _context;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IChatAdmin _chatAdmin;

    public CourseAccessRevocationService(
        ICoursesDbContext context,
        ICalendarEventPublisher calendar,
        IChatAdmin chatAdmin)
    {
        _context = context;
        _calendar = calendar;
        _chatAdmin = chatAdmin;
    }

    public async Task<Result<string>> RevokeAccessAsync(
        Guid courseId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.CourseEnrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId
                                   && e.StudentId == studentId
                                   && e.Status == EnrollmentStatus.Active, cancellationToken);

        if (enrollment == null)
            return Result.Success<string>("Доступ к курсу уже отозван.");

        enrollment.Status = EnrollmentStatus.Dropped;
        await _context.SaveChangesAsync(cancellationToken);

        await _calendar.DeleteByCourseForUserAsync(courseId, studentId, cancellationToken);
        await _chatAdmin.RemoveParticipantAsync(courseId.ToString(), studentId, cancellationToken);

        return Result.Success<string>("Доступ к курсу отозван.");
    }
}
