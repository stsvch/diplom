namespace Tests.Application.DTOs;

public class TestAttemptDetailDto : TestAttemptDto
{
    public List<TestResponseDto> Responses { get; set; } = new();
    public List<QuestionDto>? Questions { get; set; }
}
