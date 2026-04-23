using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Courses.Infrastructure.Services;

public class EnrollmentReadService : IEnrollmentReadService
{
    private readonly ICoursesDbContext _context;

    public EnrollmentReadService(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetActiveStudentIdsAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseEnrollments
            .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetActiveCourseIdsForStudentAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseEnrollments
            .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);
    }
}
