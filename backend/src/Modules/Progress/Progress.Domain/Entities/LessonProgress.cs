using EduPlatform.Shared.Domain;

namespace Progress.Domain.Entities;

public class LessonProgress : BaseEntity
{
    public Guid LessonId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
