using Content.Application.DTOs;
using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.CreateLessonBlock;

public record CreateLessonBlockCommand(
    Guid LessonId,
    LessonBlockType Type,
    LessonBlockData Data,
    LessonBlockSettings? Settings = null
) : IRequest<Result<LessonBlockDto>>;
