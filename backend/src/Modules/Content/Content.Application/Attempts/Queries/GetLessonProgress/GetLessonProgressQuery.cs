// GetLessonProgressQuery.cs

using Content.Application.DTOs;
using MediatR;

namespace Content.Application.Attempts.Queries.GetLessonProgress;

// Тип record: ключевой элемент файла GetLessonProgressQuery.cs.
public record GetLessonProgressQuery(Guid LessonId, Guid UserId) : IRequest<LessonProgressDto>;
