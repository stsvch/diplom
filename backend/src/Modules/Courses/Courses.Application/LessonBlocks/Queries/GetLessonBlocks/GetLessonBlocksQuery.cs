using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.LessonBlocks.Queries.GetLessonBlocks;

public record GetLessonBlocksQuery(Guid LessonId) : IRequest<List<LessonBlockDto>>;
