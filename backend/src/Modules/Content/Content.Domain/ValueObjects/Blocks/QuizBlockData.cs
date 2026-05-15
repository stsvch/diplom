// QuizBlockData.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

// Value object class: описывает структуру JSON-данных блока урока.
public class QuizBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Quiz;
    public Guid TestId { get; set; }
}
