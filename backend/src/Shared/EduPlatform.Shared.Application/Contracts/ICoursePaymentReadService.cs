// Файл: ICoursePaymentReadService.cs
namespace EduPlatform.Shared.Application.Contracts;

// Межмодульный контракт CoursePaymentInfo передаёт минимальные данные между контекстами модулей.
public sealed record CoursePaymentInfo(
    Guid CourseId,
    string Title,
    string TeacherId,
    string TeacherName,
    decimal? Price,
    bool IsFree,
    bool IsPublished,
    bool IsArchived);

// Сервис чтения ICoursePaymentReadService собирает модель чтения для API без изменения состояния.
public interface ICoursePaymentReadService
{
    Task<CoursePaymentInfo?> GetCoursePaymentInfoAsync(Guid courseId, CancellationToken cancellationToken = default);
}
