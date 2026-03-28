using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;

namespace Tests.Application.Tests.Queries.GetTestSubmissions;

public record GetTestSubmissionsQuery(Guid TestId, string CreatedById) : IRequest<Result<List<TestAttemptDto>>>;
