// Файл: IUserDeletionGuard.cs
namespace EduPlatform.Shared.Application.Contracts;

// Межмодульный контракт UserDeletionCheckResult передаёт минимальные данные между контекстами модулей.
public record UserDeletionCheckResult(
    bool CanDelete,
    IReadOnlyList<string> BlockingReasons);

// Интерфейс IUserDeletionGuard задаёт контракт сервиса между слоями или модулями.
public interface IUserDeletionGuard
{
    Task<UserDeletionCheckResult> CheckAsync(string userId, CancellationToken cancellationToken = default);
}
