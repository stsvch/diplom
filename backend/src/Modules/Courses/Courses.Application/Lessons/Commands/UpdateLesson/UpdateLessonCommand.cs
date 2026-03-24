using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.UpdateLesson;

public record UpdateLessonCommand(
    Guid Id,
    string Title,
    string? Description,
    int? Duration,
    bool? IsPublished
) : IRequest<Result<LessonDto>>;
