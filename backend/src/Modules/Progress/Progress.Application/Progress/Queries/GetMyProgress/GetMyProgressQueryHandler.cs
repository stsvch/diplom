using MediatR;
using Microsoft.EntityFrameworkCore;
using Progress.Application.DTOs;
using Progress.Application.Interfaces;

namespace Progress.Application.Progress.Queries.GetMyProgress;

public class GetMyProgressQueryHandler : IRequestHandler<GetMyProgressQuery, MyProgressDto>
{
    private readonly IProgressDbContext _context;

    public GetMyProgressQueryHandler(IProgressDbContext context)
    {
        _context = context;
    }

    public async Task<MyProgressDto> Handle(GetMyProgressQuery request, CancellationToken cancellationToken)
    {
        var progresses = await _context.LessonProgresses
            .Where(p => p.StudentId == request.StudentId && p.IsCompleted)
            .ToListAsync(cancellationToken);

        // Return raw progress; controller enriches with course structure
        return new MyProgressDto
        {
            Courses = progresses
                .GroupBy(p => p.LessonId)
                .Select(g => new CourseProgressDto
                {
                    CourseId = Guid.Empty, // enriched by controller
                    TotalLessons = 0,
                    CompletedLessons = g.Count(),
                    ProgressPercent = 0
                })
                .ToList()
        };
    }
}
