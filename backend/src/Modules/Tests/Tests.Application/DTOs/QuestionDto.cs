using Tests.Domain.Enums;

namespace Tests.Application.DTOs;

public class QuestionDto
{
    public Guid Id { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int OrderIndex { get; set; }
    public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
}
