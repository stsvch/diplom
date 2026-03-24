using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.DeleteModule;

public record DeleteModuleCommand(Guid Id) : IRequest<Result<string>>;
