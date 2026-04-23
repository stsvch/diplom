using Content.Application.DTOs;
using MediatR;

namespace Content.Application.LessonBlocks.Queries.GetLessonBlocks;

public record GetLessonBlocksQuery(Guid LessonId) : IRequest<List<LessonBlockDto>>;
