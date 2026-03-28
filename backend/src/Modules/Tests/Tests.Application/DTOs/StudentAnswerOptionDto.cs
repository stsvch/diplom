namespace Tests.Application.DTOs;

public class StudentAnswerOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string? MatchingPairValue { get; set; }
}
