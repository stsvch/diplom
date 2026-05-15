// ReorderLessonsCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.ReorderLessons;

// Тип record: ключевой элемент файла ReorderLessonsCommand.cs.
public record ReorderLessonsCommand(Guid ModuleId, List<Guid> OrderedIds) : IRequest<Result<string>>;
