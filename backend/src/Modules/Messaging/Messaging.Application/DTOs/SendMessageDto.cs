// SendMessageDto.cs
namespace Messaging.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public class SendMessageDto
{
    public string ChatId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<AttachmentDto>? Attachments { get; set; }
}
