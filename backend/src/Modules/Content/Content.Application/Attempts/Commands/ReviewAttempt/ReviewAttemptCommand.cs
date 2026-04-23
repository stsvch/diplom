using Content.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Attempts.Commands.ReviewAttempt;

public record ReviewAttemptCommand(
    Guid AttemptId,
    Guid ReviewerId,
    decimal Score,
    string? Comment
) : IRequest<Result<LessonBlockAttemptDto>>;
