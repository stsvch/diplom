using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Queries.GetTestById;

public record GetTestByIdQuery(Guid Id) : IRequest<Result<TestDetailDto>>;
