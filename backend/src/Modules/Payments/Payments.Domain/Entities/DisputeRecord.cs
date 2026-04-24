using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class DisputeRecord : BaseEntity, IAuditableEntity
{
    public Guid PaymentAttemptId { get; set; }
    public Guid? CoursePurchaseId { get; set; }
    public Guid? TeacherSettlementId { get; set; }
    public Guid? PayoutRecordId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string Provider { get; set; } = "Stripe";
    public string ProviderDisputeId { get; set; } = string.Empty;
    public string? ProviderPaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public decimal AppliedGrossAmount { get; set; }
    public decimal TeacherNetDisputeAmount { get; set; }
    public string Currency { get; set; } = "usd";
    public DisputeRecordStatus Status { get; set; } = DisputeRecordStatus.UnderReview;
    public string? Reason { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? EvidenceDueBy { get; set; }
    public DateTime? FundsWithdrawnAt { get; set; }
    public DateTime? FundsReinstatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? LedgerAppliedAt { get; set; }
    public DateTime? LedgerRestoredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
