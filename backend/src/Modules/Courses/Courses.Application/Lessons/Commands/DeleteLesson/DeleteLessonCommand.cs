// DeleteLessonCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.DeleteLesson;

// Тип record: ключевой элемент файла DeleteLessonCommand.cs.
public record DeleteLessonCommand(Guid Id) : IRequest<Result<string>>;
