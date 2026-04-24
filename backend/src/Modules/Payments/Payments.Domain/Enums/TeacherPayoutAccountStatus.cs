namespace Payments.Domain.Enums;

public enum TeacherPayoutAccountStatus
{
    NotStarted = 0,
    OnboardingStarted = 1,
    PendingVerification = 2,
    Ready = 3,
    Restricted = 4,
    Rejected = 5
}
