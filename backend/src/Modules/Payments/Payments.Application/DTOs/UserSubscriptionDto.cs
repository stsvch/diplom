namespace Payments.Application.DTOs;

public record UserSubscriptionDto(
    Guid Id,
    Guid SubscriptionPlanId,
    string PlanName,
    decimal Price,
    string Currency,
    string Status,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    DateTime? CanceledAt,
    DateTime StartedAt,
    DateTime? EndedAt);
