using EduPlatform.Shared.Domain;

namespace Payments.Domain.Entities;

public class PaymentMethodRef : BaseEntity, IAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Provider { get; set; } = "Stripe";
    public string ProviderCustomerId { get; set; } = string.Empty;
    public string ProviderPaymentMethodId { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Last4 { get; set; }
    public int? ExpMonth { get; set; }
    public int? ExpYear { get; set; }
    public bool IsDefault { get; set; }
    public DateTime? RemovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
