namespace Tests.Application.DTOs;

public class AnswerOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
    public string? MatchingPairValue { get; set; }
}
