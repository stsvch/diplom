using Assignments.Application.DTOs;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetSubmissions;

public record GetSubmissionsQuery(Guid AssignmentId, string TeacherId) : IRequest<List<SubmissionDto>>;
