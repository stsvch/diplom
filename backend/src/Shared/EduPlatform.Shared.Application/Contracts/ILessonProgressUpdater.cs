// Файл: ILessonProgressUpdater.cs
namespace EduPlatform.Shared.Application.Contracts;

// Интерфейс ILessonProgressUpdater задаёт контракт сервиса между слоями или модулями.
public interface ILessonProgressUpdater
{
    Task MarkLessonCompletedAsync(Guid lessonId, Guid userId, CancellationToken cancellationToken = default);
}
