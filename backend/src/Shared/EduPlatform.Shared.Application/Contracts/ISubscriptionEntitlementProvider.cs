namespace EduPlatform.Shared.Application.Contracts;

public enum LiveSlotKind
{
    Individual = 0,
    Group = 1
}

public record UserEntitlements(
    bool HasActiveSubscription,
    string? PlanName,
    int IndividualSlotsPerMonth,
    int IndividualSlotsUsed,
    int GroupSlotsPerMonth,
    int GroupSlotsUsed,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd)
{
    public int IndividualSlotsRemaining => Math.Max(0, IndividualSlotsPerMonth - IndividualSlotsUsed);
    public int GroupSlotsRemaining => Math.Max(0, GroupSlotsPerMonth - GroupSlotsUsed);

    public bool CanBook(LiveSlotKind kind) => kind switch
    {
        LiveSlotKind.Individual => HasActiveSubscription && IndividualSlotsRemaining > 0,
        LiveSlotKind.Group => HasActiveSubscription && GroupSlotsRemaining > 0,
        _ => false
    };
}

public record SlotUsageRecord(
    string UserId,
    LiveSlotKind Kind,
    Guid BookingId);

public interface ISubscriptionEntitlementProvider
{
    Task<UserEntitlements> GetForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task RecordUsageAsync(SlotUsageRecord record, CancellationToken cancellationToken = default);

    Task RefundUsageAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
