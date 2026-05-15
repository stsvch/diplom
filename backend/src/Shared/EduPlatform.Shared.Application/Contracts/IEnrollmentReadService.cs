// Файл: IEnrollmentReadService.cs
namespace EduPlatform.Shared.Application.Contracts;

// Сервис чтения IEnrollmentReadService собирает модель чтения для API без изменения состояния.
public interface IEnrollmentReadService
{
    Task<IReadOnlyList<string>> GetActiveStudentIdsAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetActiveCourseIdsForStudentAsync(string studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetActiveStudentIdsForTeacherAsync(string teacherId, CancellationToken cancellationToken = default);
}
