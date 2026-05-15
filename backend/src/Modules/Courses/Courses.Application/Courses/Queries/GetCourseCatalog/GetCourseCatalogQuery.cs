// GetCourseCatalogQuery.cs

using Courses.Application.DTOs;
using Courses.Domain.Enums;
using EduPlatform.Shared.Application.Models;
using MediatR;

namespace Courses.Application.Courses.Queries.GetCourseCatalog;

// Тип record: ключевой элемент файла GetCourseCatalogQuery.cs.
public record GetCourseCatalogQuery(
    Guid? DisciplineId,
    bool? IsFree,
    CourseLevel? Level,
    string? Search,
    string? SortBy,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<CourseListDto>>;
