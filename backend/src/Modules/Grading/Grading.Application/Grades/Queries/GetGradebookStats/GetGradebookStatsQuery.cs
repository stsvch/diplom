using Grading.Application.DTOs;
using MediatR;

namespace Grading.Application.Grades.Queries.GetGradebookStats;

public record GetGradebookStatsQuery(Guid CourseId) : IRequest<GradebookStatsDto>;
