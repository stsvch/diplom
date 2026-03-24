using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

public class Course : BaseEntity, IAuditableEntity
{
    public Guid DisciplineId { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public bool IsFree { get; set; }
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public CourseOrderType OrderType { get; set; }
    public bool HasGrading { get; set; }
    public string? ImageUrl { get; set; }
    public CourseLevel Level { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Discipline Discipline { get; set; } = null!;
    public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
}
