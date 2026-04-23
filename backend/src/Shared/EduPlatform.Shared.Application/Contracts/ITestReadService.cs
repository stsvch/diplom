using EduPlatform.Shared.Domain.Enums;

namespace EduPlatform.Shared.Application.Contracts;

public record TestDeadlineInfo(
    Guid TestId,
    Guid CourseId,
    string Title,
    DateTime? Deadline);

public interface ITestReadService
{
    Task<IReadOnlyList<TestDeadlineInfo>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, DeadlineStatus>> GetStatusesAsync(
        IReadOnlyCollection<Guid> testIds,
        string studentId,
        CancellationToken cancellationToken = default);
}
