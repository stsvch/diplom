namespace Payments.Domain.Enums;

public enum TeacherSettlementStatus
{
    PendingHold = 0,
    ReadyForPayout = 1,
    InPayout = 2,
    PaidOut = 3,
    Reversed = 4,
    Canceled = 5
}
