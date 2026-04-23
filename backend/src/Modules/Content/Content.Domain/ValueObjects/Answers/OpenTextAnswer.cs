using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

public class OpenTextAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.OpenText;
    public string Text { get; set; } = string.Empty;
}
