using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Queries.GetCourseStats;

public class GetCourseStatsQueryHandler : IRequestHandler<GetCourseStatsQuery, Result<CourseStatsDto>>
{
    private readonly ICoursesDbContext _context;

    public GetCourseStatsQueryHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CourseStatsDto>> Handle(GetCourseStatsQuery request, CancellationToken cancellationToken)
    {
        var courses = _context.Courses.AsNoTracking();
        var total = await courses.CountAsync(cancellationToken);
        var published = await courses.CountAsync(c => c.IsPublished && !c.IsArchived, cancellationToken);
        var drafts = await courses.CountAsync(c => !c.IsPublished && !c.IsArchived, cancellationToken);
        var archived = await courses.CountAsync(c => c.IsArchived, cancellationToken);
        var enrollments = await _context.CourseEnrollments
            .AsNoTracking()
            .CountAsync(e => e.Status == EnrollmentStatus.Active, cancellationToken);
        var disciplines = await _context.Disciplines.AsNoTracking().CountAsync(cancellationToken);

        return Result.Success(new CourseStatsDto
        {
            Total = total,
            Published = published,
            Drafts = drafts,
            Archived = archived,
            TotalEnrollments = enrollments,
            Disciplines = disciplines,
        });
    }
}
