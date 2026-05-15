// GetStudentGradesQuery.cs

using Grading.Application.DTOs;
using MediatR;

namespace Grading.Application.Grades.Queries.GetStudentGrades;

/// <summary>
/// CQRS-запрос GetStudentGradesQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetStudentGradesQuery(string StudentId) : IRequest<List<GradeDto>>;
