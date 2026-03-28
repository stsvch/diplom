using EduPlatform.Shared.Domain;
using Notifications.Domain.Enums;

namespace Notifications.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public string? LinkUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
