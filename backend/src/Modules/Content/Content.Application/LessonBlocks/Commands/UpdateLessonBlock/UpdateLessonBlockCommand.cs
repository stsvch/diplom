using Content.Application.DTOs;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.UpdateLessonBlock;

public record UpdateLessonBlockCommand(
    Guid Id,
    LessonBlockData Data,
    LessonBlockSettings? Settings = null
) : IRequest<Result<LessonBlockDto>>;
