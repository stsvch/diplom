// GetCourseStatsQuery.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Queries.GetCourseStats;

// Тип record: ключевой элемент файла GetCourseStatsQuery.cs.
public record GetCourseStatsQuery() : IRequest<Result<CourseStatsDto>>;
