// Файл: ITeacherCoursesReadService.cs
namespace EduPlatform.Shared.Application.Contracts;

// Межмодульный контракт TeacherCourseInfo передаёт минимальные данные между контекстами модулей.
public record TeacherCourseInfo(Guid CourseId, string TeacherId, string Title);

// Сервис чтения ITeacherCoursesReadService собирает модель чтения для API без изменения состояния.
public interface ITeacherCoursesReadService
{
    Task<IReadOnlyList<TeacherCourseInfo>> GetPublishedCoursesByTeacherIdsAsync(
        IReadOnlyCollection<string> teacherIds,
        CancellationToken cancellationToken = default);
}
