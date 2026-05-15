// TestAttemptStartDto.cs

namespace Tests.Application.DTOs;

/// <summary>
/// DTO описывает данные TestAttemptStartDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class TestAttemptStartDto
{
    public Guid AttemptId { get; set; }
    public List<StudentQuestionDto> Questions { get; set; } = new();
    public int? TimeLimitMinutes { get; set; }
    public int AttemptNumber { get; set; }
}
