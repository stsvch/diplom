using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class SubscriptionPaymentAttempt : BaseEntity, IAuditableEntity
{
    public Guid SubscriptionPlanId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public SubscriptionBillingInterval BillingInterval { get; set; } = SubscriptionBillingInterval.Month;
    public int BillingIntervalCount { get; set; } = 1;
    public string Provider { get; set; } = "Stripe";
    public SubscriptionPaymentAttemptStatus Status { get; set; } = SubscriptionPaymentAttemptStatus.Initiated;
    public string? ProviderCustomerId { get; set; }
    public string? ProviderSessionId { get; set; }
    public string? ProviderSubscriptionId { get; set; }
    public string? FailureMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
