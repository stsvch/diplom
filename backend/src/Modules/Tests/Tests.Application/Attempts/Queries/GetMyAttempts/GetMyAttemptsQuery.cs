using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Queries.GetMyAttempts;

public record GetMyAttemptsQuery(Guid TestId, string StudentId) : IRequest<Result<List<TestAttemptDto>>>;
