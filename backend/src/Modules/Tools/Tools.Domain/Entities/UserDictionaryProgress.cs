using EduPlatform.Shared.Domain;
using Tools.Domain.Enums;

namespace Tools.Domain.Entities;

public class UserDictionaryProgress : BaseEntity, IAuditableEntity
{
    public Guid WordId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsKnown { get; set; }
    public int ReviewCount { get; set; }
    public int HardCount { get; set; }
    public int RepeatLaterCount { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public DictionaryReviewOutcome? LastOutcome { get; set; }
    public DateTime? NextReviewAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DictionaryWord Word { get; set; } = null!;
}
