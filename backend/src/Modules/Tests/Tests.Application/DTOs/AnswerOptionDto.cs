// AnswerOptionDto.cs

namespace Tests.Application.DTOs;

/// <summary>
/// DTO описывает данные AnswerOptionDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class AnswerOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
    public string? MatchingPairValue { get; set; }
}
