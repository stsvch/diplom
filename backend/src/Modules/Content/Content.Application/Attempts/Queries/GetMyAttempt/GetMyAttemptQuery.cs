// GetMyAttemptQuery.cs

using Content.Application.DTOs;
using MediatR;

namespace Content.Application.Attempts.Queries.GetMyAttempt;

// Тип record: ключевой элемент файла GetMyAttemptQuery.cs.
public record GetMyAttemptQuery(Guid BlockId, Guid UserId) : IRequest<LessonBlockAttemptDto?>;
