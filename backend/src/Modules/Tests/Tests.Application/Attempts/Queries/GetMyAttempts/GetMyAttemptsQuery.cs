// GetMyAttemptsQuery.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Queries.GetMyAttempts;

/// <summary>
/// CQRS-запрос GetMyAttemptsQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetMyAttemptsQuery(Guid TestId, string StudentId) : IRequest<Result<List<TestAttemptDto>>>;
