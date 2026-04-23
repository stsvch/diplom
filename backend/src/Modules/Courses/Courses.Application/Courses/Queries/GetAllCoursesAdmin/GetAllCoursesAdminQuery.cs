using EduPlatform.Shared.Application.Models;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Queries.GetAllCoursesAdmin;

public record GetAllCoursesAdminQuery(
    string? Search,
    string? Status, // "published" | "draft" | "archived"
    Guid? DisciplineId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<AdminCourseDto>>>;
