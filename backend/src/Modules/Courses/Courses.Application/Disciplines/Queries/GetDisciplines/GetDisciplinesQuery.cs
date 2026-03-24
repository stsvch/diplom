using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Disciplines.Queries.GetDisciplines;

public record GetDisciplinesQuery : IRequest<List<DisciplineDto>>;
