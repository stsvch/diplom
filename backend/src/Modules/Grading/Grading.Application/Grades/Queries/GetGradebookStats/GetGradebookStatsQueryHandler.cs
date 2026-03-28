using Grading.Application.DTOs;
using Grading.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Grades.Queries.GetGradebookStats;

public class GetGradebookStatsQueryHandler : IRequestHandler<GetGradebookStatsQuery, GradebookStatsDto>
{
    private readonly IGradingDbContext _context;

    public GetGradebookStatsQueryHandler(IGradingDbContext context)
    {
        _context = context;
    }

    public async Task<GradebookStatsDto> Handle(GetGradebookStatsQuery request, CancellationToken cancellationToken)
    {
        var grades = await _context.Grades
            .Where(g => g.CourseId == request.CourseId)
            .ToListAsync(cancellationToken);

        if (!grades.Any())
        {
            return new GradebookStatsDto
            {
                StudentCount = 0,
                AverageScore = 0,
                TotalSubmissions = grades.Count,
                PassingCount = 0
            };
        }

        var studentIds = grades.Select(g => g.StudentId).Distinct().ToList();
        var totalSubmissions = grades.Count;

        var studentAverages = grades
            .GroupBy(g => g.StudentId)
            .Select(group =>
            {
                var avg = group.Average(g => g.MaxScore > 0 ? (g.Score / g.MaxScore * 100) : 0);
                return (StudentId: group.Key, Average: avg);
            })
            .ToList();

        var overallAverage = studentAverages.Any()
            ? (decimal)studentAverages.Average(s => s.Average)
            : 0;

        var passingCount = studentAverages.Count(s => s.Average >= 60);

        return new GradebookStatsDto
        {
            StudentCount = studentIds.Count,
            AverageScore = Math.Round(overallAverage, 2),
            TotalSubmissions = totalSubmissions,
            PassingCount = passingCount
        };
    }
}
