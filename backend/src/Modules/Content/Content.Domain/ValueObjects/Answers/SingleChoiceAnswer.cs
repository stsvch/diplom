using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

public class SingleChoiceAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.SingleChoice;
    public string SelectedOptionId { get; set; } = string.Empty;
}
