// UserEntitlementsDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
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
