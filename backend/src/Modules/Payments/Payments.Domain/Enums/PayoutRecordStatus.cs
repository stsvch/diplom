// PayoutRecordStatus.cs
namespace Payments.Domain.Enums;

// Основной тип файла описывает часть модуля и его публичный контракт.
public enum PayoutRecordStatus
{
    Queued = 0,
    SubmittedToProvider = 1,
    Paid = 2,
    Failed = 3,
    Reversed = 4,
    Canceled = 5
}
