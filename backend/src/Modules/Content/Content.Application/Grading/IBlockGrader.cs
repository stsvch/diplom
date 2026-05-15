// IBlockGrader.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Grading;

// Результат автопроверки блока: баллы, максимум, корректность, необходимость ручной проверки и фидбек.
public record GradeResult(decimal Score, decimal MaxScore, bool IsCorrect, bool NeedsReview, string? Feedback = null);

// Тип interface: ключевой элемент файла IBlockGrader.cs.
public interface IBlockGrader
{
    LessonBlockType SupportedType { get; }
    GradeResult Grade(LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings);
}

public interface IBlockGraderRegistry
{
    GradeResult Grade(LessonBlockType type, LessonBlockData data, LessonBlockAnswer answer, LessonBlockSettings settings);
}
