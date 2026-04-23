using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class OpenTextBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.OpenText;
    public string Instruction { get; set; } = string.Empty;
    public string? Prompt { get; set; }
    public List<string> HelperWords { get; set; } = new();
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public OpenTextLengthUnit Unit { get; set; } = OpenTextLengthUnit.Chars;
}

public enum OpenTextLengthUnit
{
    Chars = 0,
    Words = 1
}
