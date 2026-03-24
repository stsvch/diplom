using Courses.Application.DTOs;
using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.UpdateCourse;

public record UpdateCourseCommand(
    Guid Id,
    string TeacherId,
    Guid DisciplineId,
    string Title,
    string Description,
    decimal? Price,
    bool IsFree,
    CourseOrderType OrderType,
    bool HasGrading,
    CourseLevel Level,
    string? ImageUrl,
    string? Tags
) : IRequest<Result<CourseDetailDto>>;
