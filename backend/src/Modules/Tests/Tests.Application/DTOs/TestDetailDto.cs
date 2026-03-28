namespace Tests.Application.DTOs;

public class TestDetailDto : TestDto
{
    public List<QuestionDto> Questions { get; set; } = new();
}
