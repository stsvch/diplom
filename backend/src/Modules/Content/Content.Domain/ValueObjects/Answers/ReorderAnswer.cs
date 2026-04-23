using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

public class ReorderAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.Reorder;
    public List<string> Order { get; set; } = new();
}
