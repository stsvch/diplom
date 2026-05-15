// GetPendingSubmissionsQuery.cs

using Assignments.Application.DTOs;
using MediatR;

namespace Assignments.Application.Assignments.Queries.GetPendingSubmissions;

/// <summary>
/// Запрос хранит id преподавателя и отдаёт очередь работ на проверку.
/// </summary>
public record GetPendingSubmissionsQuery(string TeacherId) : IRequest<List<SubmissionDto>>;
