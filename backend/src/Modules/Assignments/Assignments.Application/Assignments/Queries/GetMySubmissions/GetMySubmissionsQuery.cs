using Assignments.Application.DTOs;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetMySubmissions;

public record GetMySubmissionsQuery(Guid AssignmentId, string StudentId) : IRequest<List<SubmissionDto>>;
