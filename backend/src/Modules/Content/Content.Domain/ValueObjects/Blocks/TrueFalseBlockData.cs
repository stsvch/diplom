using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class TrueFalseBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.TrueFalse;
    public string? Instruction { get; set; }
    public List<TrueFalseStatement> Statements { get; set; } = new();
}

public class TrueFalseStatement
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsTrue { get; set; }
}
