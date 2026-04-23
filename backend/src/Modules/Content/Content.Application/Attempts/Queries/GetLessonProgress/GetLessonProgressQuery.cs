using Content.Application.DTOs;
using MediatR;

namespace Content.Application.Attempts.Queries.GetLessonProgress;

public record GetLessonProgressQuery(Guid LessonId, Guid UserId) : IRequest<LessonProgressDto>;
