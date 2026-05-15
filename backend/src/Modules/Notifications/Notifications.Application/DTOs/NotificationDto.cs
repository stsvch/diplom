// Файл: NotificationDto.cs
using EduPlatform.Shared.Domain.Enums;

namespace Notifications.Application.DTOs;

// DTO NotificationDto переносит данные наружу без раскрытия доменной сущности.
public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? LinkUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
