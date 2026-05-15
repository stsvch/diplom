// GetCourseByIdQuery.cs

using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Queries.GetCourseById;

// Тип record: ключевой элемент файла GetCourseByIdQuery.cs.
public record GetCourseByIdQuery(Guid Id, string? UserId, string? UserRole) : IRequest<Result<CourseDetailDto>>;
