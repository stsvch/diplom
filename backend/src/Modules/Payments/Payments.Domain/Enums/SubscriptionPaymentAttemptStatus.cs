namespace Payments.Domain.Enums;

public enum SubscriptionPaymentAttemptStatus
{
    Initiated = 0,
    PendingProvider = 1,
    Succeeded = 2,
    Failed = 3,
    Canceled = 4,
    Expired = 5
}
