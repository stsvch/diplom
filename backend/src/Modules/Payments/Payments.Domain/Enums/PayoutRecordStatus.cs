namespace Payments.Domain.Enums;

public enum PayoutRecordStatus
{
    Queued = 0,
    SubmittedToProvider = 1,
    Paid = 2,
    Failed = 3,
    Reversed = 4,
    Canceled = 5
}
