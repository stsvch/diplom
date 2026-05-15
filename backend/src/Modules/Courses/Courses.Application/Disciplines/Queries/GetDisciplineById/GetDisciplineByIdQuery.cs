// GetDisciplineByIdQuery.cs

using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Queries.GetDisciplineById;

// Тип record: ключевой элемент файла GetDisciplineByIdQuery.cs.
public record GetDisciplineByIdQuery(Guid Id) : IRequest<Result<DisciplineDto>>;
