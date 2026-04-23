using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class FileBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.File;
    public Guid AttachmentId { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
}
