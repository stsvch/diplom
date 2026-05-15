// GetLessonBlocksQuery.cs

using Content.Application.DTOs;
using MediatR;

namespace Content.Application.LessonBlocks.Queries.GetLessonBlocks;

// Тип record: ключевой элемент файла GetLessonBlocksQuery.cs.
public record GetLessonBlocksQuery(Guid LessonId) : IRequest<List<LessonBlockDto>>;
