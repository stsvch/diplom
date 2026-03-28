using Assignments.Domain.Enums;
using EduPlatform.Shared.Domain;

namespace Assignments.Domain.Entities;

public class AssignmentSubmission : BaseEntity
{
    public Guid AssignmentId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public string? Content { get; set; }
    public DateTime SubmittedAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public int? Score { get; set; }
    public string? TeacherComment { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? GradedById { get; set; }

    public Assignment Assignment { get; set; } = null!;
}
