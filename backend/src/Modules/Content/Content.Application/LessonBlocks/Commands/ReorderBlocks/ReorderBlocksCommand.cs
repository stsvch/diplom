// ReorderBlocksCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.ReorderBlocks;

// Тип record: ключевой элемент файла ReorderBlocksCommand.cs.
public record ReorderBlocksCommand(Guid LessonId, List<Guid> OrderedIds) : IRequest<Result<string>>;
