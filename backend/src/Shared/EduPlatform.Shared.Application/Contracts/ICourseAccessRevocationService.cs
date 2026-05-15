// Файл: ICourseAccessRevocationService.cs
using EduPlatform.Shared.Domain;

namespace EduPlatform.Shared.Application.Contracts;

// Интерфейс ICourseAccessRevocationService задаёт контракт сервиса между слоями или модулями.
public interface ICourseAccessRevocationService
{
    Task<Result<string>> RevokeAccessAsync(
        Guid courseId,
        string studentId,
        CancellationToken cancellationToken = default);
}
