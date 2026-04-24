using EduPlatform.Shared.Domain;

namespace Payments.Domain.Entities;

public class UserPaymentProfile : BaseEntity, IAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Provider { get; set; } = "Stripe";
    public string ProviderCustomerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
