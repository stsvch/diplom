namespace EduPlatform.Shared.Application.Contracts;

public interface ILessonProgressUpdater
{
    Task MarkLessonCompletedAsync(Guid lessonId, Guid userId, CancellationToken cancellationToken = default);
}
