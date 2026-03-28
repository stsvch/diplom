using EduPlatform.Shared.Domain;
using MediatR;
using Progress.Application.DTOs;

namespace Progress.Application.Progress.Commands.CompleteLesson;

public record CompleteLessonCommand(Guid LessonId, string StudentId) : IRequest<Result<LessonProgressDto>>;
