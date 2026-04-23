using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading;

public record GradeResult(decimal Score, decimal MaxScore, bool IsCorrect, bool NeedsReview, string? Feedback = null);

public interface IBlockGrader
{
    LessonBlockType SupportedType { get; }
    GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings);
}

public interface IBlockGraderRegistry
{
    GradeResult Grade(LessonBlockType type, LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings);
}
