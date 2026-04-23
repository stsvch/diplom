using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using EduPlatform.Shared.Domain;

namespace Content.Domain.Entities;

public class LessonBlockAttempt : BaseEntity
{
    public Guid BlockId { get; set; }
    public Guid UserId { get; set; }
    public LessonBlockAnswer Answers { get; set; } = null!;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public bool IsCorrect { get; set; }
    public bool NeedsReview { get; set; }
    public int AttemptsUsed { get; set; }
    public LessonBlockAttemptStatus Status { get; set; } = LessonBlockAttemptStatus.Submitted;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewerComment { get; set; }

    public LessonBlock Block { get; set; } = null!;
}
