namespace EduPlatform.Shared.Application.Contracts;

public record UserDeletionCheckResult(
    bool CanDelete,
    IReadOnlyList<string> BlockingReasons);

public interface IUserDeletionGuard
{
    Task<UserDeletionCheckResult> CheckAsync(string userId, CancellationToken cancellationToken = default);
}
