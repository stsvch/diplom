using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Queries.GetDisciplineById;

public record GetDisciplineByIdQuery(Guid Id) : IRequest<Result<DisciplineDto>>;
