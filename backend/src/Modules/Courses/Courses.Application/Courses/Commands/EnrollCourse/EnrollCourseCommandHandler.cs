using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.EnrollCourse;

public class EnrollCourseCommandHandler : IRequestHandler<EnrollCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;
    private readonly ICourseAccessProvisioningService _courseAccessProvisioningService;

    public EnrollCourseCommandHandler(
        ICoursesDbContext context,
        ICourseAccessProvisioningService courseAccessProvisioningService)
    {
        _context = context;
        _courseAccessProvisioningService = courseAccessProvisioningService;
    }

    public async Task<Result<string>> Handle(EnrollCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync([request.CourseId], cancellationToken);
        if (course == null)
            return Result.Failure<string>("Курс не найден.");

        if (!course.IsPublished || course.IsArchived)
            return Result.Failure<string>("Курс недоступен для записи.");

        if (!course.IsFree)
            return Result.Failure<string>("Курс платный. Используйте оплату курса.");

        var existingEnrollment = await _context.CourseEnrollments
            .FirstOrDefaultAsync(e => e.CourseId == request.CourseId
                                   && e.StudentId == request.StudentId
                                   && e.Status == EnrollmentStatus.Active, cancellationToken);

        if (existingEnrollment != null)
            return Result.Failure<string>("Вы уже записаны на этот курс.");

        return await _courseAccessProvisioningService.GrantAccessAsync(
            request.CourseId,
            request.StudentId,
            request.StudentName,
            cancellationToken);
    }
}
