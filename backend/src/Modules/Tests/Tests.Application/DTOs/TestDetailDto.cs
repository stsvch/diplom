// TestDetailDto.cs

namespace Tests.Application.DTOs;

/// <summary>
/// DTO описывает данные TestDetailDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class TestDetailDto : TestDto
{
    public List<QuestionDto> Questions { get; set; } = new();
}
