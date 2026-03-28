using Assignments.Application.DTOs;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetPendingSubmissions;

public record GetPendingSubmissionsQuery(string TeacherId) : IRequest<List<SubmissionDto>>;
