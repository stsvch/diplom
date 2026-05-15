// CalendarSlotDto.cs

using Scheduling.Domain.Enums;

namespace Scheduling.Application.DTOs;

/// <summary>
/// Виртуальный или материализованный слот для отображения в календаре учителя/студента.
/// </summary>
public class CalendarSlotDto
{
    public Guid AvailabilityId { get; set; }
    public Guid? SlotId { get; set; }            // null = ещё не материализован
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SessionType SessionType { get; set; }
    public int MaxStudents { get; set; }
    public int BookedCount { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? RequiredCourseId { get; set; }
    public bool IsBookedByCurrentUser { get; set; }
    public bool IsAvailable => BookedCount < MaxStudents;
}
