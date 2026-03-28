namespace Tests.Application.DTOs;

public class TestResponseDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public List<string>? SelectedOptionIds { get; set; }
    public string? TextAnswer { get; set; }
    public bool? IsCorrect { get; set; }
    public int? Points { get; set; }
    public string? TeacherComment { get; set; }
}
