using EduPlatform.Shared.Domain;

namespace Payments.Domain.Entities;

public class SubscriptionAllocationLine : BaseEntity, IAuditableEntity
{
    public Guid SubscriptionAllocationRunId { get; set; }
    public Guid SubscriptionInvoiceId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public decimal AllocationWeight { get; set; }
    public decimal ProgressPercent { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal PlatformCommissionAmount { get; set; }
    public decimal ProviderFeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Currency { get; set; } = "usd";
    public Guid? PayoutRecordId { get; set; }
    public DateTime AvailableAt { get; set; }
    public DateTime? PaidOutAt { get; set; }
    public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
