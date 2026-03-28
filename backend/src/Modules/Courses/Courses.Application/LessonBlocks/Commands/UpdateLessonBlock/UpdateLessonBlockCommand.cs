using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.LessonBlocks.Commands.UpdateLessonBlock;

public record UpdateLessonBlockCommand(
    Guid Id,
    string? TextContent,
    string? VideoUrl,
    Guid? TestId = null,
    Guid? AssignmentId = null
) : IRequest<Result<LessonBlockDto>>;
