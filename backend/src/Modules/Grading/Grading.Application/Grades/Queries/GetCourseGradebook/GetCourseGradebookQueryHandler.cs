using Grading.Application.DTOs;
using Grading.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Grades.Queries.GetCourseGradebook;

public class GetCourseGradebookQueryHandler : IRequestHandler<GetCourseGradebookQuery, GradebookDto>
{
    private readonly IGradingDbContext _context;

    public GetCourseGradebookQueryHandler(IGradingDbContext context)
    {
        _context = context;
    }

    public async Task<GradebookDto> Handle(GetCourseGradebookQuery request, CancellationToken cancellationToken)
    {
        var grades = await _context.Grades
            .Where(g => g.CourseId == request.CourseId)
            .OrderBy(g => g.StudentId)
            .ThenBy(g => g.GradedAt)
            .ToListAsync(cancellationToken);

        var studentGroups = grades
            .GroupBy(g => g.StudentId)
            .Select(group =>
            {
                var gradeDtos = group.Select(g => new GradeDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    CourseId = g.CourseId,
                    SourceType = g.SourceType,
                    Title = g.Title,
                    Score = g.Score,
                    MaxScore = g.MaxScore,
                    Comment = g.Comment,
                    GradedAt = g.GradedAt
                }).ToList();

                var avg = gradeDtos.Count > 0
                    ? gradeDtos.Average(g => g.MaxScore > 0 ? (g.Score / g.MaxScore * 100) : 0)
                    : 0;

                return new StudentGradesDto
                {
                    StudentId = group.Key,
                    StudentName = group.Key, // will be enriched by controller if needed
                    Grades = gradeDtos,
                    AverageScore = Math.Round((decimal)avg, 2)
                };
            })
            .ToList();

        return new GradebookDto
        {
            CourseId = request.CourseId,
            CourseName = string.Empty,
            Students = studentGroups
        };
    }
}
