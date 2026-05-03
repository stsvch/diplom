using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

public class CourseModule : BaseEntity
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; }

    public Course Course { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<CourseItem> Items { get; set; } = new List<CourseItem>();
}
