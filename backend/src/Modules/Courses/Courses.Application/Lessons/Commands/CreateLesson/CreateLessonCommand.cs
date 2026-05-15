// CreateLessonCommand.cs

using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.CreateLesson;

// Тип record: ключевой элемент файла CreateLessonCommand.cs.
public record CreateLessonCommand(
    Guid ModuleId,
    string Title,
    string? Description,
    int? Duration
) : IRequest<Result<LessonDto>>;
