using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class BannerBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Banner;
    public string Title { get; set; } = string.Empty;
    public string? BgColor { get; set; }
    public string? TextColor { get; set; }
    public string? ImageUrl { get; set; }
}
