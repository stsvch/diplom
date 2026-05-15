// TeacherSettlementStatus.cs
namespace Payments.Domain.Enums;

// Основной тип файла описывает часть модуля и его публичный контракт.
public enum TeacherSettlementStatus
{
    PendingHold = 0,
    ReadyForPayout = 1,
    InPayout = 2,
    PaidOut = 3,
    Reversed = 4,
    Canceled = 5
}
