using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class AssignmentBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Assignment;
    public Guid AssignmentId { get; set; }
}
