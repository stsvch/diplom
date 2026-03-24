using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Modules.Queries.GetCourseModules;

public class GetCourseModulesQueryHandler : IRequestHandler<GetCourseModulesQuery, List<CourseModuleDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public GetCourseModulesQueryHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CourseModuleDto>> Handle(GetCourseModulesQuery request, CancellationToken cancellationToken)
    {
        var modules = await _context.CourseModules
            .Where(m => m.CourseId == request.CourseId)
            .Include(m => m.Lessons)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<CourseModuleDto>>(modules);
    }
}
