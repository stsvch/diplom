namespace Scheduling.Application.DTOs;

public class CreateSlotDto
{
    public Guid? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsGroupSession { get; set; }
    public int MaxStudents { get; set; } = 1;
    public string? MeetingLink { get; set; }
}
