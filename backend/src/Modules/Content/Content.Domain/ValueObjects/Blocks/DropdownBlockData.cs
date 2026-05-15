// DropdownBlockData.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

// Value object class: описывает структуру JSON-данных блока урока.
public class DropdownBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Dropdown;
    public string? Instruction { get; set; }
    public List<DropdownSentence> Sentences { get; set; } = new();
}

public class DropdownSentence
{
    public string Id { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public List<DropdownSlot> Gaps { get; set; } = new();
}

public class DropdownSlot
{
    public string Id { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string Correct { get; set; } = string.Empty;
}
