// DeleteModuleCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.DeleteModule;

// Тип record: ключевой элемент файла DeleteModuleCommand.cs.
public record DeleteModuleCommand(Guid Id) : IRequest<Result<string>>;
