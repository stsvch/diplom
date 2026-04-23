namespace EduPlatform.Shared.Application.Contracts;

public interface IContentReadService
{
    Task<int> GetBlocksCountAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetBlocksCountByLessonIdsAsync(
        IEnumerable<Guid> lessonIds,
        CancellationToken cancellationToken = default);
}
