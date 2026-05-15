// SubscriptionPaymentAttemptDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
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
