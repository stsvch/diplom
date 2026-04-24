using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class RefundRecord : BaseEntity, IAuditableEntity
{
    public Guid PaymentAttemptId { get; set; }
    public Guid? CoursePurchaseId { get; set; }
    public Guid? TeacherSettlementId { get; set; }
    public Guid? PayoutRecordId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string? RequestedByAdminId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Provider { get; set; } = "Stripe";
    public string ProviderRefundId { get; set; } = string.Empty;
    public string? ProviderPaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public decimal TeacherNetRefundAmount { get; set; }
    public string Currency { get; set; } = "usd";
    public RefundRecordStatus Status { get; set; } = RefundRecordStatus.Pending;
    public string? Reason { get; set; }
    public string? FailureMessage { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? LedgerAppliedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
