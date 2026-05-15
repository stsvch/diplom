// TestResponseDto.cs

namespace Tests.Application.DTOs;

/// <summary>
/// DTO описывает данные TestResponseDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
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
