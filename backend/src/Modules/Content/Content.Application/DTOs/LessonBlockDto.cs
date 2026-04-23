using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.DTOs;

public class LessonBlockDto
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public int OrderIndex { get; set; }
    public LessonBlockType Type { get; set; }
    public LessonBlockData Data { get; set; } = null!;
    public LessonBlockSettings Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
