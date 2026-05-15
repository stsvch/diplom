// ChatDto.cs
namespace Messaging.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public class ChatDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? OwnerId { get; set; }
    public bool IsArchived { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
