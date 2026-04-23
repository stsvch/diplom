using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.UnenrollCourse;

public class UnenrollCourseCommandHandler : IRequestHandler<UnenrollCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IChatAdmin _chatAdmin;

    public UnenrollCourseCommandHandler(
        ICoursesDbContext context,
        ICalendarEventPublisher calendar,
        IChatAdmin chatAdmin)
    {
        _context = context;
        _calendar = calendar;
        _chatAdmin = chatAdmin;
    }

    public async Task<Result<string>> Handle(UnenrollCourseCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.CourseEnrollments
            .FirstOrDefaultAsync(e => e.CourseId == request.CourseId
                                   && e.StudentId == request.StudentId
                                   && e.Status == EnrollmentStatus.Active, cancellationToken);

        if (enrollment == null)
            return Result.Failure<string>("Вы не записаны на этот курс.");

        enrollment.Status = EnrollmentStatus.Dropped;
        await _context.SaveChangesAsync(cancellationToken);

        await _calendar.DeleteByCourseForUserAsync(request.CourseId, request.StudentId, cancellationToken);

        await _chatAdmin.RemoveParticipantAsync(request.CourseId.ToString(), request.StudentId, cancellationToken);

        return Result.Success<string>("Вы отписаны от курса.");
    }
}
