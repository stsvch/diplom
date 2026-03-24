namespace Courses.Application.DTOs;

public class CourseModuleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; }
    public int LessonsCount { get; set; }
    public List<LessonDto> Lessons { get; set; } = new();
}
