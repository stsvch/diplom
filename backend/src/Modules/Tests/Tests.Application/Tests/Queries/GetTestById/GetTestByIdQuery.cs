// GetTestByIdQuery.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Queries.GetTestById;

/// <summary>
/// CQRS-запрос GetTestByIdQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetTestByIdQuery(Guid Id) : IRequest<Result<TestDetailDto>>;
