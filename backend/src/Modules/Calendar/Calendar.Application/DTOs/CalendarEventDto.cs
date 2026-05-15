// CalendarEventDto.cs
using EduPlatform.Shared.Domain.Enums;

namespace Calendar.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public class CalendarEventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? EventTime { get; set; }
    public CalendarEventType Type { get; set; }
    public Guid? CourseId { get; set; }
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public DeadlineStatus? Status { get; set; }
}
