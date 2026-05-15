// AudioBlockData.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

// Value object class: описывает структуру JSON-данных блока урока.
public class AudioBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Audio;
    public string Url { get; set; } = string.Empty;
    public string? Transcript { get; set; }
    public int? DurationSeconds { get; set; }
}
