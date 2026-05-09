namespace EduPlatform.Shared.Application.Contracts;

public record CompletedBookingInfo(
    Guid BookingId,
    string StudentId,
    string TeacherId,
    string TeacherName,
    LiveSlotKind Kind,
    DateTime SessionStart,
    DateTime CompletedAt);

public interface ICompletedBookingReadService
{
    Task<IReadOnlyList<CompletedBookingInfo>> GetCompletedBetweenAsync(
        string studentId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);
}
