using Content.Domain.ValueObjects.Answers;

namespace Content.Application.DTOs;

public class CodeExerciseRunDto
{
    public Guid Id { get; set; }
    public Guid BlockId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? AttemptId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool Ok { get; set; }
    public string? GlobalError { get; set; }
    public List<CodeTestCaseResult> Results { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int? BlockOrderIndex { get; set; }
    public string? BlockLabel { get; set; }
    public string? AttemptStatus { get; set; }
    public decimal? AttemptScore { get; set; }
    public decimal? AttemptMaxScore { get; set; }
    public bool? AttemptNeedsReview { get; set; }
    public DateTime? AttemptReviewedAt { get; set; }
    public string? AttemptReviewerComment { get; set; }
    public int? AttemptAttemptsUsed { get; set; }
}
