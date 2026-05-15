// CreateDisciplineCommand.cs

using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.CreateDiscipline;

// Тип record: ключевой элемент файла CreateDisciplineCommand.cs.
public record CreateDisciplineCommand(
    string Name,
    string? Description,
    string? ImageUrl
) : IRequest<Result<DisciplineDto>>;
