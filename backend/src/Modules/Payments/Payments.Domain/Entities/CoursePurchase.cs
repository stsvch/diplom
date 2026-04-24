using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class CoursePurchase : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public Guid PaymentAttemptId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public CoursePurchaseStatus Status { get; set; } = CoursePurchaseStatus.Active;
    public DateTime PurchasedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
