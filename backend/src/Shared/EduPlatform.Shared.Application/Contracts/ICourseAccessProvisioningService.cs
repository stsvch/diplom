// Файл: ICourseAccessProvisioningService.cs
using EduPlatform.Shared.Domain;

namespace EduPlatform.Shared.Application.Contracts;

// Интерфейс ICourseAccessProvisioningService задаёт контракт сервиса между слоями или модулями.
public interface ICourseAccessProvisioningService
{
    Task<Result<string>> GrantAccessAsync(
        Guid courseId,
        string studentId,
        string studentName,
        CancellationToken cancellationToken = default);
}
