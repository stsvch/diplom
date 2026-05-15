// StudentQuestionDto.cs

using Tests.Domain.Enums;

namespace Tests.Application.DTOs;

/// <summary>
/// DTO описывает данные StudentQuestionDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class StudentQuestionDto
{
    public Guid Id { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int OrderIndex { get; set; }
    public List<StudentAnswerOptionDto> AnswerOptions { get; set; } = new();
}
