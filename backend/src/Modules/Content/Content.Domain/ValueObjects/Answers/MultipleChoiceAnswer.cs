// MultipleChoiceAnswer.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

// Value object class: описывает структуру ответа студента для проверки блока.
public class MultipleChoiceAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.MultipleChoice;
    public List<string> SelectedOptionIds { get; set; } = new();
}
