using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Modules.Queries.GetCourseModules;

public record GetCourseModulesQuery(Guid CourseId) : IRequest<List<CourseModuleDto>>;
