namespace EduPlatform.Shared.Application.Contracts;

public interface IChatAdmin
{
    Task CreateCourseChatAsync(
        string courseId,
        string courseName,
        string teacherId,
        string teacherName,
        CancellationToken cancellationToken = default);

    Task AddParticipantAsync(
        string courseId,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    Task RemoveParticipantAsync(
        string courseId,
        string userId,
        CancellationToken cancellationToken = default);

    Task DeleteCourseChatAsync(
        string courseId,
        CancellationToken cancellationToken = default);

    Task ArchiveCourseChatAsync(
        string courseId,
        CancellationToken cancellationToken = default);
}
