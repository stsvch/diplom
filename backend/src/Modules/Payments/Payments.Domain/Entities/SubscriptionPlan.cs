using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class SubscriptionPlan : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "usd";
    public SubscriptionBillingInterval BillingInterval { get; set; } = SubscriptionBillingInterval.Month;
    public int BillingIntervalCount { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public string? ProviderProductId { get; set; }
    public string? ProviderPriceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
