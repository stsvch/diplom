using Content.Domain.Enums;

namespace Content.Application.DTOs;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public AttachmentEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
