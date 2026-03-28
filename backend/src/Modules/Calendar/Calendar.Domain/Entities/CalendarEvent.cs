using Calendar.Domain.Enums;
using EduPlatform.Shared.Domain;

namespace Calendar.Domain.Entities;

public class CalendarEvent : BaseEntity
{
    public string? UserId { get; set; }
    public Guid? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? EventTime { get; set; }
    public CalendarEventType Type { get; set; }
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
