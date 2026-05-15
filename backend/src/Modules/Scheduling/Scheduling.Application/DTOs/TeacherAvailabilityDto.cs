// TeacherAvailabilityDto.cs

using Scheduling.Domain.Enums;

namespace Scheduling.Application.DTOs;

/// <summary>
/// DTO описывает данные TeacherAvailabilityDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public class TeacherAvailabilityDto
{
    public Guid Id { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public AvailabilityKind Kind { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public DateOnly? SpecificDate { get; set; }
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
    public Guid? RequiredCourseId { get; set; }
    public bool IsActive { get; set; }
}
