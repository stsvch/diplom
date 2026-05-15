// StudentAnswerOptionDto.cs

namespace Tests.Application.DTOs;

/// <summary>
/// DTO описывает данные StudentAnswerOptionDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class StudentAnswerOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? MatchingPairValue { get; set; }
}
