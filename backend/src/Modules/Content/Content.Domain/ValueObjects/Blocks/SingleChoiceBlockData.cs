using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class SingleChoiceBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.SingleChoice;
    public string? Instruction { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<ChoiceOption> Options { get; set; } = new();
}

public class ChoiceOption
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
