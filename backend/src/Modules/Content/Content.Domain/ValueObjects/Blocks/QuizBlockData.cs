using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class QuizBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Quiz;
    public Guid TestId { get; set; }
}
