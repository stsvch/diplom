namespace EduPlatform.Shared.Application.Contracts;

public interface ITeacherPayoutReadService
{
    Task<bool> IsTeacherReadyForPaidCoursesAsync(string teacherId, CancellationToken cancellationToken = default);
}
