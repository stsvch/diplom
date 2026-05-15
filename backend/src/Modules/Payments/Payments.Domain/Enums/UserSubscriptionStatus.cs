// UserSubscriptionStatus.cs
namespace Payments.Domain.Enums;

// Основной тип файла описывает часть модуля и его публичный контракт.
public enum UserSubscriptionStatus
{
    PendingActivation = 0,
    Active = 1,
    Incomplete = 2,
    PastDue = 3,
    Canceled = 4,
    Unpaid = 5,
    Paused = 6,
    Trialing = 7
}
