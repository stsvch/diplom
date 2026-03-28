using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Queries.GetAttempt;

public record GetAttemptQuery(Guid AttemptId) : IRequest<Result<TestAttemptDetailDto>>;
