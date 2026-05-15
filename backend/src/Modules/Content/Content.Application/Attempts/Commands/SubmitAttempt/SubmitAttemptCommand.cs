// SubmitAttemptCommand.cs

using Content.Application.DTOs;
using Content.Domain.ValueObjects.Answers;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Attempts.Commands.SubmitAttempt;

// Тип record: ключевой элемент файла SubmitAttemptCommand.cs.
public record SubmitAttemptCommand(
    Guid BlockId,
    Guid UserId,
    LessonBlockAnswer Answers
) : IRequest<Result<SubmitAttemptResultDto>>;
