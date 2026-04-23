using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class TextBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Text;
    public string Html { get; set; } = string.Empty;
}
