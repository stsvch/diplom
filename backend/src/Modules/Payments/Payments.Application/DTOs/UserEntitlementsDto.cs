namespace Payments.Application.DTOs;

public record UserEntitlementsDto(
    bool HasActiveSubscription,
    string? PlanName,
    int IndividualSlotsPerMonth,
    int IndividualSlotsUsed,
    int IndividualSlotsRemaining,
    int GroupSlotsPerMonth,
    int GroupSlotsUsed,
    int GroupSlotsRemaining,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd);
