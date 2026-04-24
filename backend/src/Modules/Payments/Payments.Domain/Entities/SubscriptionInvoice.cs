using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class SubscriptionInvoice : BaseEntity, IAuditableEntity
{
    public Guid SubscriptionPlanId { get; set; }
    public Guid? UserSubscriptionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Provider { get; set; } = "Stripe";
    public string ProviderInvoiceId { get; set; } = string.Empty;
    public string? ProviderSubscriptionId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public string Currency { get; set; } = "usd";
    public SubscriptionInvoiceStatus Status { get; set; } = SubscriptionInvoiceStatus.Open;
    public string? BillingReason { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? FailureMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
