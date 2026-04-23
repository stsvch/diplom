namespace EduPlatform.Shared.Application.Contracts;

public interface IEnrollmentReadService
{
    Task<IReadOnlyList<string>> GetActiveStudentIdsAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetActiveCourseIdsForStudentAsync(string studentId, CancellationToken cancellationToken = default);
}
