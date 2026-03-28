namespace Tests.Application.DTOs;

public class TestAttemptStartDto
{
    public Guid AttemptId { get; set; }
    public List<StudentQuestionDto> Questions { get; set; } = new();
    public int? TimeLimitMinutes { get; set; }
    public int AttemptNumber { get; set; }
}
