using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class PayoutRecord : BaseEntity, IAuditableEntity
{
    public string TeacherId { get; set; } = string.Empty;
    public string Provider { get; set; } = "Stripe";
    public string? ProviderAccountId { get; set; }
    public string? ProviderTransferId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public int SettlementsCount { get; set; }
    public int AllocationLinesCount { get; set; }
    public PayoutRecordStatus Status { get; set; } = PayoutRecordStatus.Queued;
    public DateTime RequestedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
