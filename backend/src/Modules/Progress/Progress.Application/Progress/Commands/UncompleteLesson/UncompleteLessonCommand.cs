using EduPlatform.Shared.Domain;
using MediatR;

namespace Progress.Application.Progress.Commands.UncompleteLesson;

public record UncompleteLessonCommand(Guid LessonId, string StudentId) : IRequest<Result>;
