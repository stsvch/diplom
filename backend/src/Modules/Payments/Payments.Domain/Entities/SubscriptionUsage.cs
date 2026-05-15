// SubscriptionUsage.cs
using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

// Основной тип файла описывает часть модуля и его публичный контракт.
public class SubscriptionUsage : BaseEntity
{
    public Guid UserSubscriptionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public SlotUsageType Type { get; set; }
    public DateTime PeriodStart { get; set; }
    public Guid SourceBookingId { get; set; }
    public DateTime UsedAt { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime? RefundedAt { get; set; }
}
