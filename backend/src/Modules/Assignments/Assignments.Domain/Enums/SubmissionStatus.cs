// SubmissionStatus.cs

namespace Assignments.Domain.Enums;

/// <summary>
/// Статусы описывают жизненный цикл сдачи: от отправки и проверки до оценки или возврата на доработку.
/// </summary>
public enum SubmissionStatus
{
    Submitted,
    UnderReview,
    Graded,
    ReturnedForRevision
}
