using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class PaymentAttempt : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Provider { get; set; } = "Stripe";
    public PaymentAttemptStatus Status { get; set; } = PaymentAttemptStatus.Initiated;
    public bool SavePaymentMethodRequested { get; set; }
    public string? ProviderCustomerId { get; set; }
    public string? ProviderSessionId { get; set; }
    public string? ProviderPaymentIntentId { get; set; }
    public string? ProviderChargeId { get; set; }
    public string? ProviderPaymentMethodId { get; set; }
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
