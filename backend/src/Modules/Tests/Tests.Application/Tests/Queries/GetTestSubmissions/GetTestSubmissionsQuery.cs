// GetTestSubmissionsQuery.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Queries.GetTestSubmissions;

/// <summary>
/// CQRS-запрос GetTestSubmissionsQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetTestSubmissionsQuery(Guid TestId, string CreatedById) : IRequest<Result<List<TestAttemptDto>>>;
