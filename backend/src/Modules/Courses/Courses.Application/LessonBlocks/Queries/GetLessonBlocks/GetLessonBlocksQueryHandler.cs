using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.LessonBlocks.Queries.GetLessonBlocks;

public class GetLessonBlocksQueryHandler : IRequestHandler<GetLessonBlocksQuery, List<LessonBlockDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public GetLessonBlocksQueryHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LessonBlockDto>> Handle(GetLessonBlocksQuery request, CancellationToken cancellationToken)
    {
        var blocks = await _context.LessonBlocks
            .Where(b => b.LessonId == request.LessonId)
            .OrderBy(b => b.OrderIndex)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<LessonBlockDto>>(blocks);
    }
}
