// Файл: ILessonContentCleaner.cs
namespace EduPlatform.Shared.Application.Contracts;

// Интерфейс ILessonContentCleaner задаёт контракт сервиса между слоями или модулями.
public interface ILessonContentCleaner
{
    Task DeleteByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task DeleteByLessonIdsAsync(IEnumerable<Guid> lessonIds, CancellationToken cancellationToken = default);
}
