using Tests.Domain.Enums;

namespace Tests.Application.DTOs;

public class StudentQuestionDto
{
    public Guid Id { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int OrderIndex { get; set; }
    public List<StudentAnswerOptionDto> AnswerOptions { get; set; } = new();
}
