// UpdateDisciplineCommand.cs

using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.UpdateDiscipline;

// Тип record: ключевой элемент файла UpdateDisciplineCommand.cs.
public record UpdateDisciplineCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl
) : IRequest<Result<DisciplineDto>>;
