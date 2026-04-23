using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;

namespace Content.Application.DTOs;

public class LessonBlockAttemptDto
{
    public Guid Id { get; set; }
    public Guid BlockId { get; set; }
    public Guid UserId { get; set; }
    public LessonBlockAnswer Answers { get; set; } = null!;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public bool IsCorrect { get; set; }
    public bool NeedsReview { get; set; }
    public int AttemptsUsed { get; set; }
    public LessonBlockAttemptStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewerComment { get; set; }
}

public class SubmitAttemptResultDto
{
    public Guid AttemptId { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public bool IsCorrect { get; set; }
    public bool NeedsReview { get; set; }
    public int AttemptsUsed { get; set; }
    public int? AttemptsRemaining { get; set; }
    public string? Feedback { get; set; }
}

public class LessonProgressDto
{
    public Guid LessonId { get; set; }
    public int TotalBlocks { get; set; }
    public int RequiredBlocks { get; set; }
    public int CompletedBlocks { get; set; }
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public bool IsCompleted { get; set; }
}
