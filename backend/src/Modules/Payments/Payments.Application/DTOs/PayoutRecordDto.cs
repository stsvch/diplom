// PayoutRecordDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public record PayoutRecordDto(
    Guid Id,
    decimal Amount,
    string Currency,
    int SettlementsCount,
    string Status,
    string? ProviderTransferId,
    DateTime RequestedAt,
    DateTime? SubmittedAt,
    DateTime? PaidAt,
    DateTime? FailedAt,
    string? FailureMessage);
