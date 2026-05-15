// DeleteLessonBlockCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.DeleteLessonBlock;

// Тип record: ключевой элемент файла DeleteLessonBlockCommand.cs.
public record DeleteLessonBlockCommand(Guid Id) : IRequest<Result<string>>;
