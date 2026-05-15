// TestAttemptDetailDto.cs

namespace Tests.Application.DTOs;

/// <summary>
/// DTO описывает данные TestAttemptDetailDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class TestAttemptDetailDto : TestAttemptDto
{
    public List<TestResponseDto> Responses { get; set; } = new();
    public List<QuestionDto>? Questions { get; set; }
}
