// UpdateCourseCommand.cs

using Courses.Application.DTOs;
using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.UpdateCourse;

// Тип record: ключевой элемент файла UpdateCourseCommand.cs.
public record UpdateCourseCommand(
    Guid Id,
    string TeacherId,
    Guid DisciplineId,
    string Title,
    string Description,
    decimal? Price,
    bool IsFree,
    CourseOrderType OrderType,
    CourseLevel Level,
    string? ImageUrl,
    DateTime? Deadline
) : IRequest<Result<CourseDetailDto>>;
