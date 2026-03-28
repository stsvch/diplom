using Courses.Domain.Enums;

namespace Courses.Application.DTOs;

public class LessonBlockDto
{
    public Guid Id { get; set; }
    public int OrderIndex { get; set; }
    public LessonBlockType Type { get; set; }
    public string? TextContent { get; set; }
    public string? VideoUrl { get; set; }
    public Guid? TestId { get; set; }
    public Guid? AssignmentId { get; set; }
}
