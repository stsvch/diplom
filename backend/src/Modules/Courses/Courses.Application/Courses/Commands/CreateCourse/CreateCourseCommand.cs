// CreateCourseCommand.cs

using Courses.Application.DTOs;
using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.CreateCourse;

// Тип record: ключевой элемент файла CreateCourseCommand.cs.
public record CreateCourseCommand(
    string TeacherId,
    string TeacherName,
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
