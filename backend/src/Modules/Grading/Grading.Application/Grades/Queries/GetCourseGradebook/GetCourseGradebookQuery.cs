using Grading.Application.DTOs;
using MediatR;

namespace Grading.Application.Grades.Queries.GetCourseGradebook;

public record GetCourseGradebookQuery(Guid CourseId) : IRequest<GradebookDto>;
