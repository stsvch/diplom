using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using EduPlatform.Shared.Domain;

namespace Content.Domain.Entities;

public class CodeExerciseRun : BaseEntity
{
    public Guid BlockId { get; set; }
    public Guid UserId { get; set; }
    public Guid? AttemptId { get; set; }
    public CodeExerciseRunKind Kind { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool Ok { get; set; }
    public string? GlobalError { get; set; }
    public List<CodeTestCaseResult> Results { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    public LessonBlock Block { get; set; } = null!;
    public LessonBlockAttempt? Attempt { get; set; }
}
