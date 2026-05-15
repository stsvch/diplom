// GetGradebookStatsQuery.cs

using Grading.Application.DTOs;
using MediatR;

namespace Grading.Application.Grades.Queries.GetGradebookStats;

/// <summary>
/// CQRS-запрос GetGradebookStatsQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetGradebookStatsQuery(Guid CourseId) : IRequest<GradebookStatsDto>;
