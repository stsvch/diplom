// Файл: IGradeRecordWriter.cs
namespace EduPlatform.Shared.Application.Contracts;

// Межмодульный контракт GradeRecordUpsert передаёт минимальные данные между контекстами модулей.
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

// Интерфейс IGradeRecordWriter задаёт контракт сервиса между слоями или модулями.
public interface IGradeRecordWriter
{
    Task UpsertAsync(GradeRecordUpsert request, CancellationToken cancellationToken = default);

    Task DeleteByTestAttemptAsync(Guid testAttemptId, CancellationToken cancellationToken = default);

    Task DeleteByAssignmentSubmissionAsync(Guid assignmentSubmissionId, CancellationToken cancellationToken = default);
}
