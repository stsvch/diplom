// GetCourseGradebookQuery.cs

using Grading.Application.DTOs;
using MediatR;

namespace Grading.Application.Grades.Queries.GetCourseGradebook;

/// <summary>
/// CQRS-запрос GetCourseGradebookQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetCourseGradebookQuery(Guid CourseId) : IRequest<GradebookDto>;
