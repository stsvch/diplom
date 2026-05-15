// SubscriptionInvoiceStatus.cs
namespace Payments.Domain.Enums;

// Основной тип файла описывает часть модуля и его публичный контракт.
public enum SubscriptionInvoiceStatus
{
    Open = 0,
    Paid = 1,
    Failed = 2,
    Void = 3,
    Uncollectible = 4
}
