using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Queries.GetCourseById;

public record GetCourseByIdQuery(Guid Id) : IRequest<Result<CourseDetailDto>>;
