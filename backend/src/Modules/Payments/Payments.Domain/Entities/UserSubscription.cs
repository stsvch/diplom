using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class UserSubscription : BaseEntity, IAuditableEntity
{
    public Guid SubscriptionPlanId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Provider { get; set; } = "Stripe";
    public string ProviderCustomerId { get; set; } = string.Empty;
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "usd";
    public UserSubscriptionStatus Status { get; set; } = UserSubscriptionStatus.PendingActivation;
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CanceledAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
