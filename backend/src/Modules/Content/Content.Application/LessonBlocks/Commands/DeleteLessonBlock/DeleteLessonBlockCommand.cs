using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.DeleteLessonBlock;

public record DeleteLessonBlockCommand(Guid Id) : IRequest<Result<string>>;
