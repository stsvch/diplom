// Файл: IAssignmentReadService.cs
using EduPlatform.Shared.Domain.Enums;

namespace EduPlatform.Shared.Application.Contracts;

// Межмодульный контракт AssignmentDeadlineInfo передаёт минимальные данные между контекстами модулей.
public record AssignmentDeadlineInfo(
    Guid AssignmentId,
    Guid CourseId,
    string Title,
    DateTime? Deadline);

// Сервис чтения IAssignmentReadService собирает модель чтения для API без изменения состояния.
public interface IAssignmentReadService
{
    Task<IReadOnlyList<AssignmentDeadlineInfo>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, DeadlineStatus>> GetStatusesAsync(
        IReadOnlyCollection<Guid> assignmentIds,
        string studentId,
        CancellationToken cancellationToken = default);
}
