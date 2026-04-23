using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class FillGapBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.FillGap;
    public string? Instruction { get; set; }
    public List<FillGapSentence> Sentences { get; set; } = new();
}

public class FillGapSentence
{
    public string Id { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public List<FillGapSlot> Gaps { get; set; } = new();
}

public class FillGapSlot
{
    public string Id { get; set; } = string.Empty;
    public List<string> CorrectAnswers { get; set; } = new();
    public bool CaseSensitive { get; set; }
}
