// Файл: IContentReadService.cs
namespace EduPlatform.Shared.Application.Contracts;

// Сервис чтения IContentReadService собирает модель чтения для API без изменения состояния.
public interface IContentReadService
{
    Task<int> GetBlocksCountAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetBlocksCountByLessonIdsAsync(
        IEnumerable<Guid> lessonIds,
        CancellationToken cancellationToken = default);
}
