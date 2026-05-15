// PendingTestAttemptDto.cs

namespace Tests.Application.DTOs;

/// <summary>
/// DTO описывает данные PendingTestAttemptDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class PendingTestAttemptDto
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int MaxScore { get; set; }
    public int OpenQuestionsTotal { get; set; }
    public int OpenQuestionsUngraded { get; set; }
}
