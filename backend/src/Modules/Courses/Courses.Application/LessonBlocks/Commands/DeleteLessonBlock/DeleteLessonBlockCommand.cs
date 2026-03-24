using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.LessonBlocks.Commands.DeleteLessonBlock;

public record DeleteLessonBlockCommand(Guid Id) : IRequest<Result<string>>;
