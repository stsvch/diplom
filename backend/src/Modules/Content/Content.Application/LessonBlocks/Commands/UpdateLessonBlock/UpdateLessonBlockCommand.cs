using Content.Application.DTOs;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.UpdateLessonBlock;

public record UpdateLessonBlockCommand(
    Guid Id,
    LessonBlockData Data,
    LessonBlockSettings? Settings = null,
    /// <summary>
    /// Если true — требуем строгую валидацию (для перевода блока в Ready).
    /// Если false — soft-режим: блок может остаться в Draft с ошибками в JSON.
    /// </summary>
    bool RequestReady = false
) : IRequest<Result<LessonBlockDto>>;
