// UpdateLessonCommand.cs

using Courses.Application.DTOs;
using Courses.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.UpdateLesson;

// Тип record: ключевой элемент файла UpdateLessonCommand.cs.
public record UpdateLessonCommand(
    Guid Id,
    string TeacherId,
    string Title,
    string? Description,
    int? Duration,
    bool? IsPublished,
    LessonLayout? Layout = null,
    Guid? ModuleId = null
) : IRequest<Result<LessonDto>>;
