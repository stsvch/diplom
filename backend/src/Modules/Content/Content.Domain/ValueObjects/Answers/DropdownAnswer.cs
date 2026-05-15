// DropdownAnswer.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

// Value object class: описывает структуру ответа студента для проверки блока.
public class DropdownAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.Dropdown;
    public List<FillGapResponse> Responses { get; set; } = new();
}
