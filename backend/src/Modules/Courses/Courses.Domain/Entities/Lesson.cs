using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

public class Lesson : BaseEntity
{
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; }
    public int? Duration { get; set; }
    public LessonLayout Layout { get; set; } = LessonLayout.Scroll;

    public CourseModule Module { get; set; } = null!;
}

public enum LessonLayout
{
    Scroll = 0,
    Stepper = 1
}
