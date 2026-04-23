using Assignments.Application.DTOs;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetMyAssignments;

public record GetMyAssignmentsQuery(string TeacherId) : IRequest<List<AssignmentDto>>;
