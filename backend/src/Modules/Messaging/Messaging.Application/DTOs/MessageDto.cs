namespace Messaging.Application.DTOs;

public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<AttachmentDto> Attachments { get; set; } = new();
    public DateTime SentAt { get; set; }
    public List<string> ReadBy { get; set; } = new();
    public bool IsEdited { get; set; }
}
