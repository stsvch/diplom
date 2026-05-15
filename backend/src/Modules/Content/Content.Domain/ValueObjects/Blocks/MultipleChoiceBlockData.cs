// MultipleChoiceBlockData.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

// Value object class: описывает структуру JSON-данных блока урока.
public class MultipleChoiceBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.MultipleChoice;
    public string? Instruction { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<ChoiceOption> Options { get; set; } = new();
    public bool PartialCredit { get; set; } = true;
}
