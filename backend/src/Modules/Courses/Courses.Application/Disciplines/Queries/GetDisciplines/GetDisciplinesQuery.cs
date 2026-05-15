// GetDisciplinesQuery.cs

using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Disciplines.Queries.GetDisciplines;

// Тип record: ключевой элемент файла GetDisciplinesQuery.cs.
public record GetDisciplinesQuery : IRequest<List<DisciplineDto>>;
