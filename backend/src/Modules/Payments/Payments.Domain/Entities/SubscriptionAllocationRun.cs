using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class SubscriptionAllocationRun : BaseEntity, IAuditableEntity
{
    public Guid SubscriptionInvoiceId { get; set; }
    public Guid? UserSubscriptionId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal PlatformCommissionAmount { get; set; }
    public decimal ProviderFeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Strategy { get; set; } = "ProgressWeightedActiveEnrollmentsV1";
    public SubscriptionAllocationRunStatus Status { get; set; } = SubscriptionAllocationRunStatus.Applied;
    public int TeacherCount { get; set; }
    public int CourseCount { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
