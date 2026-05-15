// GetAttemptQuery.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Attempts.Queries.GetAttempt;

/// <summary>
/// CQRS-запрос GetAttemptQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetAttemptQuery(Guid AttemptId) : IRequest<Result<TestAttemptDetailDto>>;
