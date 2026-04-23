namespace EduPlatform.Shared.Application.Contracts;

public interface ILessonContentCleaner
{
    Task DeleteByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task DeleteByLessonIdsAsync(IEnumerable<Guid> lessonIds, CancellationToken cancellationToken = default);
}
