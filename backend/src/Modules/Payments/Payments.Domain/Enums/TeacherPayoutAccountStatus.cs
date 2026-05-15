// TeacherPayoutAccountStatus.cs
namespace Payments.Domain.Enums;

// Основной тип файла описывает часть модуля и его публичный контракт.
public enum TeacherPayoutAccountStatus
{
    NotStarted = 0,
    OnboardingStarted = 1,
    PendingVerification = 2,
    Ready = 3,
    Restricted = 4,
    Rejected = 5
}
