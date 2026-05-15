// Файл: ITestReadService.cs
using EduPlatform.Shared.Domain.Enums;

namespace EduPlatform.Shared.Application.Contracts;

// Межмодульный контракт TestDeadlineInfo передаёт минимальные данные между контекстами модулей.
public record TestDeadlineInfo(
    Guid TestId,
    Guid CourseId,
    string Title,
    DateTime? Deadline);

// Сервис чтения ITestReadService собирает модель чтения для API без изменения состояния.
public interface ITestReadService
{
    Task<IReadOnlyList<TestDeadlineInfo>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, DeadlineStatus>> GetStatusesAsync(
        IReadOnlyCollection<Guid> testIds,
        string studentId,
        CancellationToken cancellationToken = default);
}
