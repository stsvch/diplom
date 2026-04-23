using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Domain;

namespace Content.Domain.Entities;

public class LessonBlock : BaseEntity, IAuditableEntity
{
    public Guid LessonId { get; set; }
    public int OrderIndex { get; set; }
    public LessonBlockType Type { get; set; }
    public LessonBlockData Data { get; set; } = null!;
    public LessonBlockSettings Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
