namespace Messaging.Application.DTOs;

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
