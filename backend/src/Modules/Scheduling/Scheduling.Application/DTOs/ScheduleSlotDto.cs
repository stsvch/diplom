using Scheduling.Domain.Enums;

namespace Scheduling.Application.DTOs;

public class ScheduleSlotDto
{
    public Guid Id { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public Guid? AvailabilityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SessionType SessionType { get; set; }
    public int MaxStudents { get; set; }
    public Guid? RequiredCourseId { get; set; }
    public SlotStatus Status { get; set; }
    public string? MeetingLink { get; set; }
    public int BookedCount { get; set; }
    public List<BookingDto> Bookings { get; set; } = new();
}
