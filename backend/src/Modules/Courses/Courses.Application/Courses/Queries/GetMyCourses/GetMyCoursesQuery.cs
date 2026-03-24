using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Courses.Queries.GetMyCourses;

public record GetMyCoursesQuery(string UserId, string Role) : IRequest<List<CourseListDto>>;
