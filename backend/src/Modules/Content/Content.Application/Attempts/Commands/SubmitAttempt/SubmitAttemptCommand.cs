using Content.Application.DTOs;
using Content.Domain.ValueObjects.Answers;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Attempts.Commands.SubmitAttempt;

public record SubmitAttemptCommand(
    Guid BlockId,
    Guid UserId,
    LessonBlockAnswer Answers
) : IRequest<Result<SubmitAttemptResultDto>>;
