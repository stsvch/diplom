using EduPlatform.Shared.Domain.Enums;

namespace EduPlatform.Shared.Application.Contracts;

public record AssignmentDeadlineInfo(
    Guid AssignmentId,
    Guid CourseId,
    string Title,
    DateTime? Deadline);

public interface IAssignmentReadService
{
    Task<IReadOnlyList<AssignmentDeadlineInfo>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, DeadlineStatus>> GetStatusesAsync(
        IReadOnlyCollection<Guid> assignmentIds,
        string studentId,
        CancellationToken cancellationToken = default);
}
