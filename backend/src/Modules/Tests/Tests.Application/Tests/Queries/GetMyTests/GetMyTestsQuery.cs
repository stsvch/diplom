// GetMyTestsQuery.cs

using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Queries.GetMyTests;

/// <summary>
/// CQRS-запрос GetMyTestsQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetMyTestsQuery(string TeacherId) : IRequest<List<TestDto>>;
