namespace Payments.Domain.Enums;

public enum CoursePurchaseStatus
{
    Active = 0,
    PartiallyRefunded = 1,
    Refunded = 2,
    Revoked = 3,
    Disputed = 4
}
