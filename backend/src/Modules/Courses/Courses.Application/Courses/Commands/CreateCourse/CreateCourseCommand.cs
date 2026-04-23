using Courses.Application.DTOs;
using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.CreateCourse;

public record CreateCourseCommand(
    string TeacherId,
    string TeacherName,
    Guid DisciplineId,
    string Title,
    string Description,
    decimal? Price,
    bool IsFree,
    CourseOrderType OrderType,
    bool HasGrading,
    CourseLevel Level,
    string? ImageUrl,
    string? Tags,
    bool HasCertificate,
    DateTime? Deadline
) : IRequest<Result<CourseDetailDto>>;
