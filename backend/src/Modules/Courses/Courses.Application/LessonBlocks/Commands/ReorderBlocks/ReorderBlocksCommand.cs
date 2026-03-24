using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.LessonBlocks.Commands.ReorderBlocks;

public record ReorderBlocksCommand(Guid LessonId, List<Guid> OrderedIds) : IRequest<Result<string>>;
