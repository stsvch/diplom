// OpenTextAnswer.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

// Value object class: описывает структуру ответа студента для проверки блока.
public class OpenTextAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.OpenText;
    public string Text { get; set; } = string.Empty;
}
