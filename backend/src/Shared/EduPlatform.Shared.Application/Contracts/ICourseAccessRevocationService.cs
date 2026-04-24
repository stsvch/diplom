using EduPlatform.Shared.Domain;

namespace EduPlatform.Shared.Application.Contracts;

public interface ICourseAccessRevocationService
{
    Task<Result<string>> RevokeAccessAsync(
        Guid courseId,
        string studentId,
        CancellationToken cancellationToken = default);
}
