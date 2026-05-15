// Файл: UncompleteLessonCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Progress.Application.Progress.Commands.UncompleteLesson;

// Команда UncompleteLessonCommand переносит входные данные операции изменения состояния в MediatR.
public record UncompleteLessonCommand(Guid LessonId, string StudentId) : IRequest<Result>;
