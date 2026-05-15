// UserSubscriptionDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
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
    DateTime? EndedAt,
    int IndividualSlotsPerMonth,
    int GroupSlotsPerMonth);
