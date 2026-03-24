using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.ReorderModules;

public record ReorderModulesCommand(Guid CourseId, List<Guid> OrderedIds) : IRequest<Result<string>>;
