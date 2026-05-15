// ReorderAnswer.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

// Value object class: описывает структуру ответа студента для проверки блока.
public class ReorderAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.Reorder;
    public List<string> Order { get; set; } = new();
}
