using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class TeacherSettlement : BaseEntity, IAuditableEntity
{
    public string TeacherId { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid PaymentAttemptId { get; set; }
    public Guid CoursePurchaseId { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal ProviderFeeAmount { get; set; }
    public decimal PlatformCommissionAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal RefundedGrossAmount { get; set; }
    public decimal RefundedNetAmount { get; set; }
    public decimal DisputedGrossAmount { get; set; }
    public decimal DisputedNetAmount { get; set; }
    public string Currency { get; set; } = "usd";
    public Guid? PayoutRecordId { get; set; }
    public TeacherSettlementStatus Status { get; set; } = TeacherSettlementStatus.PendingHold;
    public DateTime AvailableAt { get; set; }
    public DateTime? PaidOutAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
