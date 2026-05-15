// TestAttempt.cs

using EduPlatform.Shared.Domain;
using Tests.Domain.Enums;

namespace Tests.Domain.Entities;

/// <summary>
/// Тип TestAttempt относится к основному сценарию модуля и документирует его публичный контракт.
/// </summary>
public class TestAttempt : BaseEntity
{
    public Guid TestId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int? Score { get; set; }
    public AttemptStatus Status { get; set; }

    public Test Test { get; set; } = null!;
    public ICollection<TestResponse> Responses { get; set; } = new List<TestResponse>();
}
