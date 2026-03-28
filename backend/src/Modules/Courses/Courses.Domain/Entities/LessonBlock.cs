using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

public class LessonBlock : BaseEntity
{
    public Guid LessonId { get; set; }
    public int OrderIndex { get; set; }
    public LessonBlockType Type { get; set; }
    public string? TextContent { get; set; }
    public string? VideoUrl { get; set; }
    public Guid? TestId { get; set; }
    public Guid? AssignmentId { get; set; }

    public Lesson Lesson { get; set; } = null!;
}
