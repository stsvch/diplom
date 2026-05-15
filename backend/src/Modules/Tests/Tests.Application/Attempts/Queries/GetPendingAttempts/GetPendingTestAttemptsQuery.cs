// GetPendingTestAttemptsQuery.cs

using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Queries.GetPendingAttempts;

/// <summary>
/// CQRS-запрос GetPendingTestAttemptsQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetPendingTestAttemptsQuery(string TeacherId) : IRequest<List<PendingTestAttemptDto>>;
