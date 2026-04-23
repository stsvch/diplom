using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

public class DropdownAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.Dropdown;
    public List<FillGapResponse> Responses { get; set; } = new();
}
