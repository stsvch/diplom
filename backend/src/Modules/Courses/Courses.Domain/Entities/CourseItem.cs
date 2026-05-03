using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

public class CourseItem : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid? ModuleId { get; set; }
    public CourseItemType Type { get; set; }
    public Guid SourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public Guid? AttachmentId { get; set; }
    public string? ResourceKind { get; set; }
    public int OrderIndex { get; set; }
    public CourseItemStatus Status { get; set; } = CourseItemStatus.Draft;
    public bool IsRequired { get; set; } = true;
    public decimal? Points { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Course Course { get; set; } = null!;
    public CourseModule? Module { get; set; }
}
