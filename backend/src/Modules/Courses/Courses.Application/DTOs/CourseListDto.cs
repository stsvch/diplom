using Courses.Domain.Enums;

namespace Courses.Application.DTOs;

public class CourseListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public CourseLevel Level { get; set; }
    public decimal? Price { get; set; }
    public bool IsFree { get; set; }
    public double? Rating { get; set; }
    public int StudentsCount { get; set; }
    public int LessonsCount { get; set; }
    public int? Duration { get; set; }
    public double? Progress { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public string? ArchiveReason { get; set; }
}
