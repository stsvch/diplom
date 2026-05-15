// SubscriptionPaymentAttemptStatus.cs
namespace Payments.Domain.Enums;

// Основной тип файла описывает часть модуля и его публичный контракт.
public enum SubscriptionPaymentAttemptStatus
{
    Initiated = 0,
    PendingProvider = 1,
    Succeeded = 2,
    Failed = 3,
    Canceled = 4,
    Expired = 5
}
