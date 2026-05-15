// TeacherAvailability.cs

using EduPlatform.Shared.Domain;
using Scheduling.Domain.Enums;

namespace Scheduling.Domain.Entities;

/// <summary>
/// Тип TeacherAvailability относится к основному сценарию модуля и документирует его публичный контракт.
/// </summary>
public class TeacherAvailability : BaseEntity, IAuditableEntity
{
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;

    public AvailabilityKind Kind { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }      // для Recurring
    public DateOnly? SpecificDate { get; set; }    // для OneOff

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public int BreakBetweenMinutes { get; set; }

    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }

    public SessionType SessionType { get; set; }
    public int MaxStudents { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MeetingLink { get; set; }

    public Guid? RequiredCourseId { get; set; }    // null = открыто всем подписчикам

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
