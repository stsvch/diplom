// GetMyAssignmentsQuery.cs

using Assignments.Application.DTOs;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetMyAssignments;

/// <summary>
/// Запрос хранит id преподавателя и возвращает только задания этого автора.
/// </summary>
public record GetMyAssignmentsQuery(string TeacherId) : IRequest<List<AssignmentDto>>;
