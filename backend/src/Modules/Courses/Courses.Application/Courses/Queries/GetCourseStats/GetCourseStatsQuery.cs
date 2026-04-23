using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Queries.GetCourseStats;

public record GetCourseStatsQuery() : IRequest<Result<CourseStatsDto>>;
