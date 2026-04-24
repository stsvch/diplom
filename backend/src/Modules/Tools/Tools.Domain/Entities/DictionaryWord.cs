using EduPlatform.Shared.Domain;

namespace Tools.Domain.Entities;

public class DictionaryWord : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public string Term { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public string? Tags { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserDictionaryProgress> ProgressEntries { get; set; } = new List<UserDictionaryProgress>();
}
