// GradeDto.cs

using Grading.Domain.Enums;

namespace Grading.Application.DTOs;

/// <summary>
/// DTO описывает данные GradeDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class GradeDto
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string? CourseName { get; set; }
    public GradeSourceType SourceType { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string? Comment { get; set; }
    public DateTime GradedAt { get; set; }
}

/// <summary>
/// DTO описывает данные StudentGradesDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class StudentGradesDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public List<GradeDto> Grades { get; set; } = new();
    public decimal AverageScore { get; set; }
}

/// DTO описывает данные GradebookDto, передаваемые наружу без прямой отдачи доменных сущностей.
public class GradebookDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public List<StudentGradesDto> Students { get; set; } = new();
}

/// <summary>
/// DTO описывает данные GradebookStatsDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class GradebookStatsDto
{
    public int StudentCount { get; set; }
    public decimal AverageScore { get; set; }
    public int TotalSubmissions { get; set; }
    public int PassingCount { get; set; }
}
