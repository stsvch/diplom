using EduPlatform.Shared.Domain;
using Grading.Domain.Enums;

namespace Grading.Domain.Entities;

public class Grade : BaseEntity, IAuditableEntity
{
    public string StudentId { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public GradeSourceType SourceType { get; set; }
    public Guid? TestAttemptId { get; set; }
    public Guid? AssignmentSubmissionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string? Comment { get; set; }
    public DateTime GradedAt { get; set; }
    public string? GradedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
