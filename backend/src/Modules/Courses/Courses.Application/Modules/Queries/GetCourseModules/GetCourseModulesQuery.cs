// GetCourseModulesQuery.cs

using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Modules.Queries.GetCourseModules;

// Тип record: ключевой элемент файла GetCourseModulesQuery.cs.
public record GetCourseModulesQuery(Guid CourseId) : IRequest<List<CourseModuleDto>>;
