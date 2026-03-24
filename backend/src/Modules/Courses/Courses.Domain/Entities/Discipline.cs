using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

public class Discipline : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
