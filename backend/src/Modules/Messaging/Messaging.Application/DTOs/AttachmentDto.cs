// AttachmentDto.cs
namespace Messaging.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public class AttachmentDto
{
    public Guid? AttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
