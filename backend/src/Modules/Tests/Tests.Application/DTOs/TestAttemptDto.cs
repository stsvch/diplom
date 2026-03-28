using Tests.Domain.Enums;

namespace Tests.Application.DTOs;

public class TestAttemptDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int MaxScore { get; set; }
    public AttemptStatus Status { get; set; }
}
