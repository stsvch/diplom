namespace Courses.Domain.Entities;

/// <summary>
/// M:N-связь между Course и Tag. Композитный ключ (CourseId, TagId).
/// </summary>
public class CourseTag
{
    public Guid CourseId { get; set; }
    public Guid TagId { get; set; }

    public Course Course { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
