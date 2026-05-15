// GetStudentGradesQueryHandler.cs

using Grading.Application.DTOs;
using Grading.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Grades.Queries.GetStudentGrades;

/// <summary>
/// Обработчик CQRS-запроса GetStudentGradesQuery: читает данные, применяет фильтры и возвращает DTO/Result.
/// </summary>
public class GetStudentGradesQueryHandler : IRequestHandler<GetStudentGradesQuery, List<GradeDto>>
{
    private readonly IGradingDbContext _context;

    public GetStudentGradesQueryHandler(IGradingDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<List<GradeDto>> Handle(GetStudentGradesQuery request, CancellationToken cancellationToken)
    {
        var grades = await _context.Grades
            .Where(g => g.StudentId == request.StudentId)
            .OrderByDescending(g => g.GradedAt)
            .ToListAsync(cancellationToken);

        return grades.Select(g => new GradeDto
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
    }
}
