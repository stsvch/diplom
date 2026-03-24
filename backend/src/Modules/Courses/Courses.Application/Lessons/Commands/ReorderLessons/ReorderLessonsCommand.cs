using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.ReorderLessons;

public record ReorderLessonsCommand(Guid ModuleId, List<Guid> OrderedIds) : IRequest<Result<string>>;
