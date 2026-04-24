namespace Payments.Domain.Enums;

public enum SubscriptionInvoiceStatus
{
    Open = 0,
    Paid = 1,
    Failed = 2,
    Void = 3,
    Uncollectible = 4
}
