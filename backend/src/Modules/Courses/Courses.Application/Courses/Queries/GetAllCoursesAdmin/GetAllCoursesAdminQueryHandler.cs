using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Queries.GetAllCoursesAdmin;

public class GetAllCoursesAdminQueryHandler : IRequestHandler<GetAllCoursesAdminQuery, Result<PagedResult<AdminCourseDto>>>
{
    private readonly ICoursesDbContext _context;

    public GetAllCoursesAdminQueryHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AdminCourseDto>>> Handle(GetAllCoursesAdminQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Include(c => c.Discipline)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(q) ||
                c.TeacherName.ToLower().Contains(q));
        }

        if (request.DisciplineId.HasValue)
            query = query.Where(c => c.DisciplineId == request.DisciplineId.Value);

        query = request.Status switch
        {
            "published" => query.Where(c => c.IsPublished && !c.IsArchived),
            "draft" => query.Where(c => !c.IsPublished && !c.IsArchived),
            "archived" => query.Where(c => c.IsArchived),
            _ => query,
        };

        var total = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                TeacherId = c.TeacherId,
                TeacherName = c.TeacherName,
                DisciplineId = c.DisciplineId,
                DisciplineName = c.Discipline.Name,
                IsPublished = c.IsPublished,
                IsArchived = c.IsArchived,
                ArchiveReason = c.ArchiveReason,
                StudentsCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                ModulesCount = c.Modules.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<AdminCourseDto>(items, total, page, pageSize));
    }
}
