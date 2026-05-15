// ScheduleSlot.cs

using EduPlatform.Shared.Domain;
using Scheduling.Domain.Enums;

namespace Scheduling.Domain.Entities;

/// <summary>
/// Тип ScheduleSlot относится к основному сценарию модуля и документирует его публичный контракт.
/// </summary>
public class ScheduleSlot : BaseEntity, IAuditableEntity
{
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;

    /// <summary>Правило расписания, из которого материализован слот. null = ручной ad-hoc.</summary>
    public Guid? AvailabilityId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public SessionType SessionType { get; set; }
    public int MaxStudents { get; set; } = 1;

    /// <summary>Если задано — забронировать может только студент с активным enrollment на этот курс.</summary>
    public Guid? RequiredCourseId { get; set; }

    public SlotStatus Status { get; set; } = SlotStatus.Available;
    public string? MeetingLink { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<SessionBooking> Bookings { get; set; } = new List<SessionBooking>();
}
