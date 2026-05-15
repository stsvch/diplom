// AssignmentBlockData.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

// Value object class: описывает структуру JSON-данных блока урока.
public class AssignmentBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Assignment;
    public Guid AssignmentId { get; set; }
}
