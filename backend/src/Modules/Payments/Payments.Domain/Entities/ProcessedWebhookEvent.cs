// ProcessedWebhookEvent.cs
using EduPlatform.Shared.Domain;

namespace Payments.Domain.Entities;

// Основной тип файла описывает часть модуля и его публичный контракт.
public class ProcessedWebhookEvent : BaseEntity, IAuditableEntity
{
    public string Provider { get; set; } = "Stripe";
    public string ProviderEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
