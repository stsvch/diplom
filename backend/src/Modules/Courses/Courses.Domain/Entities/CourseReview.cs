// CourseReview.cs

using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

// Доменная сущность class: хранит состояние, инварианты и связи бизнес-модели модуля Courses.
public class CourseReview : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Course Course { get; set; } = null!;
}
