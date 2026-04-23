using Content.Application.DTOs;
using MediatR;

namespace Content.Application.Attempts.Queries.GetLessonAttempts;

public record GetLessonAttemptsQuery(Guid LessonId, Guid? UserId = null) : IRequest<List<LessonBlockAttemptDto>>;
