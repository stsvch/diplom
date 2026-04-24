using Tools.Application.DTOs;

namespace Tools.Application.Interfaces;

public interface IGlossaryService
{
    Task<IReadOnlyList<DictionaryWordDto>> GetTeacherWordsAsync(
        string teacherId,
        Guid? courseId,
        string? search,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DictionaryWordDto>> GetStudentWordsAsync(
        string studentId,
        Guid? courseId,
        string? search,
        bool knownOnly,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DictionaryWordDto>> GetStudentReviewSessionAsync(
        string studentId,
        Guid? courseId,
        int take,
        IReadOnlyCollection<Guid>? excludeWordIds,
        CancellationToken cancellationToken = default);

    Task<DictionaryWordDto> CreateWordAsync(
        string teacherId,
        Guid courseId,
        string term,
        string translation,
        string? definition,
        string? example,
        IReadOnlyCollection<string>? tags,
        CancellationToken cancellationToken = default);

    Task<DictionaryWordDto> UpdateWordAsync(
        Guid wordId,
        string teacherId,
        Guid courseId,
        string term,
        string translation,
        string? definition,
        string? example,
        IReadOnlyCollection<string>? tags,
        CancellationToken cancellationToken = default);

    Task DeleteWordAsync(
        Guid wordId,
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<DictionaryWordDto> SetStudentProgressAsync(
        Guid wordId,
        string studentId,
        bool isKnown,
        CancellationToken cancellationToken = default);

    Task<DictionaryWordDto> ReviewWordAsync(
        Guid wordId,
        string studentId,
        string outcome,
        CancellationToken cancellationToken = default);
}
