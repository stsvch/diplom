namespace Payments.Domain.Enums;

public enum PaymentAttemptStatus
{
    Initiated = 0,
    PendingProvider = 1,
    Succeeded = 2,
    Failed = 3,
    Canceled = 4,
    Expired = 5,
    Refunded = 6,
    PartiallyRefunded = 7,
    Disputed = 8
}
