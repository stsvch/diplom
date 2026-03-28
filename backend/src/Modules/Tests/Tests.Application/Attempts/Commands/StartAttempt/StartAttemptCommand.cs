using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Commands.StartAttempt;

public record StartAttemptCommand(Guid TestId, string StudentId) : IRequest<Result<TestAttemptStartDto>>;
