using Content.Domain.Enums;
using EduPlatform.Shared.Domain;

namespace Content.Domain.Entities;

public class Attachment : BaseEntity, IAuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public AttachmentEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
