using EduPlatform.Shared.Domain;

namespace Progress.Domain.Entities;

public class CourseItemProgress : BaseEntity
{
    public Guid CourseId { get; set; }
    public Guid CourseItemId { get; set; }
    public Guid SourceId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
