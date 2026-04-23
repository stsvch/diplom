using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Queries.GetMyTests;

public record GetMyTestsQuery(string TeacherId) : IRequest<List<TestDto>>;
