using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.DeleteDiscipline;

public record DeleteDisciplineCommand(Guid Id) : IRequest<Result<string>>;
