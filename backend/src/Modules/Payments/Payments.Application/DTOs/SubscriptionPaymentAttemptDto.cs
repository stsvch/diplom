namespace Payments.Application.DTOs;

public record SubscriptionPaymentAttemptDto(
    Guid Id,
    Guid SubscriptionPlanId,
    string PlanName,
    decimal Amount,
    string Currency,
    string BillingInterval,
    int BillingIntervalCount,
    string Status,
    string? FailureMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt);
