using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class ImageBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Image;
    public string Url { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public string? Caption { get; set; }
}
