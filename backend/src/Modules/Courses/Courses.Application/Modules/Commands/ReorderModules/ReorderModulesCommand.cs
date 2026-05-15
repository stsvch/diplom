// ReorderModulesCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.ReorderModules;

// Тип record: ключевой элемент файла ReorderModulesCommand.cs.
public record ReorderModulesCommand(Guid CourseId, List<Guid> OrderedIds) : IRequest<Result<string>>;
