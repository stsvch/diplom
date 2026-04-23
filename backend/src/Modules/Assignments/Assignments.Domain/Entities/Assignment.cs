using EduPlatform.Shared.Domain;

namespace Assignments.Domain.Entities;

public class Assignment : BaseEntity, IAuditableEntity
{
    public Guid? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Criteria { get; set; }
    public DateTime? Deadline { get; set; }
    public int? MaxAttempts { get; set; }
    public int MaxScore { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}
