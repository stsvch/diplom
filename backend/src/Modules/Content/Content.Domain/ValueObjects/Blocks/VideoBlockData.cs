using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class VideoBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Video;
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? PosterUrl { get; set; }
}
