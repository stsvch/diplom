// Файл: CompleteLessonCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;
using Progress.Application.DTOs;

namespace Progress.Application.Progress.Commands.CompleteLesson;

// Команда CompleteLessonCommand переносит входные данные операции изменения состояния в MediatR.
public record CompleteLessonCommand(Guid LessonId, string StudentId) : IRequest<Result<LessonProgressDto>>;
