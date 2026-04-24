namespace Payments.Domain.Enums;

public enum DisputeRecordStatus
{
    NeedsResponse = 0,
    UnderReview = 1,
    Won = 2,
    Lost = 3,
    WarningNeedsResponse = 4,
    WarningUnderReview = 5,
    WarningClosed = 6,
    Prevented = 7
}
