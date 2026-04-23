using Content.Application.DTOs;
using MediatR;

namespace Content.Application.Attempts.Queries.GetMyAttempt;

public record GetMyAttemptQuery(Guid BlockId, Guid UserId) : IRequest<LessonBlockAttemptDto?>;
