using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Commands.SubmitAttempt;

public record SubmitAttemptCommand(Guid AttemptId, string StudentId) : IRequest<Result<TestAttemptDetailDto>>;
