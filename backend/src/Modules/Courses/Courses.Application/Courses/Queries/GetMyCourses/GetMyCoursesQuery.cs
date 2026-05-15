// GetMyCoursesQuery.cs

using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Courses.Queries.GetMyCourses;

// Тип record: ключевой элемент файла GetMyCoursesQuery.cs.
public record GetMyCoursesQuery(string UserId, string Role) : IRequest<List<CourseListDto>>;
