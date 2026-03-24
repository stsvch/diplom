namespace Courses.Application.DTOs;

public class LessonDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; }
    public int? Duration { get; set; }
    public int BlocksCount { get; set; }
}
