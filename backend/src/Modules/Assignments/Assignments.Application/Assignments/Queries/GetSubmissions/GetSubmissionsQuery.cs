// GetSubmissionsQuery.cs

using Assignments.Application.DTOs;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetSubmissions;

/// <summary>
/// Запрос хранит id задания и преподавателя, чтобы проверить авторство перед выдачей всех сдач.
/// </summary>
public record GetSubmissionsQuery(Guid AssignmentId, string TeacherId) : IRequest<List<SubmissionDto>>;
