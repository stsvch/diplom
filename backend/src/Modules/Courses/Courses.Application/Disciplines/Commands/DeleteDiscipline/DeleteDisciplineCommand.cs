// DeleteDisciplineCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.DeleteDiscipline;

// Тип record: ключевой элемент файла DeleteDisciplineCommand.cs.
public record DeleteDisciplineCommand(Guid Id) : IRequest<Result<string>>;
