using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

public class MultipleChoiceAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.MultipleChoice;
    public List<string> SelectedOptionIds { get; set; } = new();
}
