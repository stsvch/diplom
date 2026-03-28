using Grading.Application.DTOs;
using MediatR;

namespace Grading.Application.Grades.Queries.GetStudentGrades;

public record GetStudentGradesQuery(string StudentId) : IRequest<List<GradeDto>>;
