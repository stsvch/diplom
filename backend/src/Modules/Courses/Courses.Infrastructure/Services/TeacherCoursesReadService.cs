// TeacherCoursesReadService.cs

using Courses.Application.Interfaces;
using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Courses.Infrastructure.Services;

// Инфраструктурный сервис class: реализует внешний или межмодульный контракт.
public class TeacherCoursesReadService : ITeacherCoursesReadService
{
    private readonly ICoursesDbContext _context;
    private readonly ILogger<TeacherCoursesReadService> _logger;

    public TeacherCoursesReadService(
        ICoursesDbContext context,
        ILogger<TeacherCoursesReadService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TeacherCourseInfo>> GetPublishedCoursesByTeacherIdsAsync(
        IReadOnlyCollection<string> teacherIds,
        CancellationToken cancellationToken = default)
    {
        if (teacherIds.Count == 0)
            return Array.Empty<TeacherCourseInfo>();

        var published = await _context.Courses
            .Where(c => teacherIds.Contains(c.TeacherId)
                     && c.IsPublished
                     && !c.IsArchived)
            .OrderBy(c => c.Title)
            .Select(c => new TeacherCourseInfo(c.Id, c.TeacherId, c.Title))
            .ToListAsync(cancellationToken);

        if (published.Count == 0)
        {
            var diag = await _context.Courses
                .Where(c => teacherIds.Contains(c.TeacherId))
                .Select(c => new { c.Id, c.TeacherId, c.Title, c.IsPublished, c.IsArchived })
                .ToListAsync(cancellationToken);

            _logger.LogWarning(
                "TeacherCoursesReadService: no published courses for teacherIds=[{Ids}]. " +
                "Total matched (any status): {Total}. Details: {Details}",
                string.Join(",", teacherIds),
                diag.Count,
                string.Join("; ", diag.Select(d => $"{d.Title} (Id={d.Id}, IsPublished={d.IsPublished}, IsArchived={d.IsArchived}, TeacherId={d.TeacherId})")));
        }

        return published;
    }
}
