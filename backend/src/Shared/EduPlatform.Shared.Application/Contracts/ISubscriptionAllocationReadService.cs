namespace EduPlatform.Shared.Application.Contracts;

public sealed record SubscriptionAllocationCandidate(
    Guid CourseId,
    string CourseTitle,
    string TeacherId,
    string TeacherName,
    int TotalLessons,
    int CompletedLessons,
    decimal ProgressPercent);

public interface ISubscriptionAllocationReadService
{
    Task<IReadOnlyList<SubscriptionAllocationCandidate>> GetAllocationCandidatesAsync(
        string studentId,
        DateTime? periodStart,
        DateTime? periodEnd,
        CancellationToken cancellationToken = default);
}
