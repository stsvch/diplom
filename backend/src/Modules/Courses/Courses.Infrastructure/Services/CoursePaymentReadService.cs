using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Courses.Infrastructure.Services;

public class CoursePaymentReadService : ICoursePaymentReadService
{
    private readonly Persistence.CoursesDbContext _context;

    public CoursePaymentReadService(Persistence.CoursesDbContext context)
    {
        _context = context;
    }

    public async Task<CoursePaymentInfo?> GetCoursePaymentInfoAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .Where(x => x.Id == courseId)
            .Select(x => new CoursePaymentInfo(
                x.Id,
                x.Title,
                x.TeacherId,
                x.TeacherName,
                x.Price,
                x.IsFree,
                x.IsPublished,
                x.IsArchived))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
