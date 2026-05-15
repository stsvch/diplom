// SingleChoiceAnswer.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

// Value object class: описывает структуру ответа студента для проверки блока.
public class SingleChoiceAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.SingleChoice;
    public string SelectedOptionId { get; set; } = string.Empty;
}
