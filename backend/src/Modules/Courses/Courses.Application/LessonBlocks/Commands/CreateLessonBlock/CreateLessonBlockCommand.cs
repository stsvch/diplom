using Courses.Application.DTOs;
using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.LessonBlocks.Commands.CreateLessonBlock;

public record CreateLessonBlockCommand(
    Guid LessonId,
    LessonBlockType Type,
    string? TextContent,
    string? VideoUrl
) : IRequest<Result<LessonBlockDto>>;
