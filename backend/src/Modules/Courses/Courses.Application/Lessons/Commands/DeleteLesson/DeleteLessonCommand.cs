using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.DeleteLesson;

public record DeleteLessonCommand(Guid Id) : IRequest<Result<string>>;
