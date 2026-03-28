namespace Messaging.Application.DTOs;

public class SendMessageDto
{
    public string ChatId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<AttachmentDto>? Attachments { get; set; }
}
