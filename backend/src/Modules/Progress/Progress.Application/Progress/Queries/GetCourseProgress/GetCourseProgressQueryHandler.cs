using MediatR;
using Microsoft.EntityFrameworkCore;
using Progress.Application.DTOs;
using Progress.Application.Interfaces;

namespace Progress.Application.Progress.Queries.GetCourseProgress;

public class GetCourseProgressQueryHandler : IRequestHandler<GetCourseProgressQuery, CourseProgressDto>
{
    private readonly IProgressDbContext _context;

    public GetCourseProgressQueryHandler(IProgressDbContext context)
    {
        _context = context;
    }

    public async Task<CourseProgressDto> Handle(GetCourseProgressQuery request, CancellationToken cancellationToken)
    {
        var totalLessons = request.LessonIds.Count;

        if (totalLessons == 0)
            return new CourseProgressDto
            {
                CourseId = request.CourseId,
                TotalLessons = 0,
                CompletedLessons = 0,
                ProgressPercent = 0
            };

        var completedCount = await _context.LessonProgresses
            .CountAsync(p => p.StudentId == request.StudentId
                && p.IsCompleted
                && request.LessonIds.Contains(p.LessonId), cancellationToken);

        var percent = totalLessons > 0
            ? Math.Round((decimal)completedCount / totalLessons * 100, 2)
            : 0;

        return new CourseProgressDto
        {
            CourseId = request.CourseId,
            TotalLessons = totalLessons,
            CompletedLessons = completedCount,
            ProgressPercent = percent
        };
    }
}
