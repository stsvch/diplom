namespace Courses.Application.Tags;

/// <summary>
/// Синхронизирует набор тегов курса: ищет или создаёт Tag-записи и обновляет связи CourseTag.
/// </summary>
public interface ITagSynchronizer
{
    Task SyncAsync(Guid courseId, IEnumerable<string>? rawTagNames, CancellationToken cancellationToken);
}
