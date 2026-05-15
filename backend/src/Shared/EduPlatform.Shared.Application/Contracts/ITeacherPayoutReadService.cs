// Файл: ITeacherPayoutReadService.cs
namespace EduPlatform.Shared.Application.Contracts;

// Сервис чтения ITeacherPayoutReadService собирает модель чтения для API без изменения состояния.
public interface ITeacherPayoutReadService
{
    Task<bool> IsTeacherReadyForPaidCoursesAsync(string teacherId, CancellationToken cancellationToken = default);
}
