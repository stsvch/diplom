// TeacherSettlementSummaryDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public record TeacherSettlementSummaryDto(
    decimal TotalGrossAmount,
    decimal TotalNetAmount,
    decimal PendingNetAmount,
    decimal ReadyForPayoutNetAmount,
    decimal InPayoutNetAmount,
    decimal PaidOutNetAmount,
    int SettlementsCount,
    string Currency);
