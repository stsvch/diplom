// GetLessonAttemptsQuery.cs

using Content.Application.DTOs;
using MediatR;

namespace Content.Application.Attempts.Queries.GetLessonAttempts;

// Тип record: ключевой элемент файла GetLessonAttemptsQuery.cs.
public record GetLessonAttemptsQuery(Guid LessonId, Guid? UserId = null) : IRequest<List<LessonBlockAttemptDto>>;
