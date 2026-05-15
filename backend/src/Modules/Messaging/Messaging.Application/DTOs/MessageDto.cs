// MessageDto.cs
namespace Messaging.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<AttachmentDto> Attachments { get; set; } = new();
    public DateTime SentAt { get; set; }
    public bool IsEdited { get; set; }
}
