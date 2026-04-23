namespace EduPlatform.Shared.Application.Contracts;

public record GradeRecordUpsert(
    string StudentId,
    Guid CourseId,
    string SourceType,
    Guid? TestAttemptId,
    Guid? AssignmentSubmissionId,
    string Title,
    decimal Score,
    decimal MaxScore,
    string? Comment,
    DateTime GradedAt,
    string? GradedById);

public interface IGradeRecordWriter
{
    Task UpsertAsync(GradeRecordUpsert request, CancellationToken cancellationToken = default);

    Task DeleteByTestAttemptAsync(Guid testAttemptId, CancellationToken cancellationToken = default);

    Task DeleteByAssignmentSubmissionAsync(Guid assignmentSubmissionId, CancellationToken cancellationToken = default);
}
