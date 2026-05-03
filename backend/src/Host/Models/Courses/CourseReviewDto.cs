namespace EduPlatform.Host.Models.Courses;

public sealed class CourseReviewDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed record UpsertCourseReviewRequest(int Rating, string? Comment);
