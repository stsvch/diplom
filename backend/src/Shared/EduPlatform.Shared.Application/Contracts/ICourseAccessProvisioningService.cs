using EduPlatform.Shared.Domain;

namespace EduPlatform.Shared.Application.Contracts;

public interface ICourseAccessProvisioningService
{
    Task<Result<string>> GrantAccessAsync(
        Guid courseId,
        string studentId,
        string studentName,
        CancellationToken cancellationToken = default);
}
