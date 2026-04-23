using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class ReorderBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Reorder;
    public string? Instruction { get; set; }
    public List<ReorderItem> Items { get; set; } = new();
    public List<string> CorrectOrder { get; set; } = new();
    public bool AllOrNothing { get; set; } = true;
}

public class ReorderItem
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
