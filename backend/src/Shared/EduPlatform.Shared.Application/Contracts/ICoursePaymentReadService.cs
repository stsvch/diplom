namespace EduPlatform.Shared.Application.Contracts;

public sealed record CoursePaymentInfo(
    Guid CourseId,
    string Title,
    string TeacherId,
    string TeacherName,
    decimal? Price,
    bool IsFree,
    bool IsPublished,
    bool IsArchived);

public interface ICoursePaymentReadService
{
    Task<CoursePaymentInfo?> GetCoursePaymentInfoAsync(Guid courseId, CancellationToken cancellationToken = default);
}
