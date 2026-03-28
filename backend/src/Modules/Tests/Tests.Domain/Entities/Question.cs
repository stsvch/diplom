using EduPlatform.Shared.Domain;
using Tests.Domain.Enums;

namespace Tests.Domain.Entities;

public class Question : BaseEntity
{
    public Guid TestId { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int OrderIndex { get; set; }

    public Test Test { get; set; } = null!;
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}
