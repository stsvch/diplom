using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.EnrollCourse;

public class EnrollCourseCommandHandler : IRequestHandler<EnrollCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public EnrollCourseCommandHandler(ICoursesDbContext context)
    {
        _context = context;
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

        return Result.Success<string>("Вы записаны на курс.");
    }
}
