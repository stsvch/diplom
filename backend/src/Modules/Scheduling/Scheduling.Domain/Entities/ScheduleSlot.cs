using EduPlatform.Shared.Domain;
using Scheduling.Domain.Enums;

namespace Scheduling.Domain.Entities;

public class ScheduleSlot : BaseEntity, IAuditableEntity
{
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public Guid? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsGroupSession { get; set; }
    public int MaxStudents { get; set; } = 1;
    public SlotStatus Status { get; set; } = SlotStatus.Available;
    public string? MeetingLink { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<SessionBooking> Bookings { get; set; } = new List<SessionBooking>();
}
