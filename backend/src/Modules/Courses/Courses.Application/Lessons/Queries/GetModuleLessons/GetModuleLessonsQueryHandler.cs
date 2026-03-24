using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Lessons.Queries.GetModuleLessons;

public class GetModuleLessonsQueryHandler : IRequestHandler<GetModuleLessonsQuery, List<LessonDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public GetModuleLessonsQueryHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LessonDto>> Handle(GetModuleLessonsQuery request, CancellationToken cancellationToken)
    {
        var lessons = await _context.Lessons
            .Where(l => l.ModuleId == request.ModuleId)
            .Include(l => l.Blocks)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<LessonDto>>(lessons);
    }
}
