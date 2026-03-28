using EduPlatform.Shared.Domain;

namespace Tests.Domain.Entities;

public class AnswerOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
    public string? MatchingPairValue { get; set; }

    public Question Question { get; set; } = null!;
}
