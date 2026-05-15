// GetMySubmissionsQuery.cs

using Assignments.Application.DTOs;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetMySubmissions;

/// <summary>
/// Запрос хранит id задания и студента, чтобы показать именно его попытки.
/// </summary>
public record GetMySubmissionsQuery(Guid AssignmentId, string StudentId) : IRequest<List<SubmissionDto>>;
