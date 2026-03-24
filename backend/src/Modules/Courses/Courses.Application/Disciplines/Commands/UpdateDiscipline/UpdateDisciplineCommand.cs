using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.UpdateDiscipline;

public record UpdateDisciplineCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl
) : IRequest<Result<DisciplineDto>>;
