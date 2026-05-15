// PaymentAttemptDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public record PaymentAttemptDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    string Currency,
    string Status,
    string? ProviderChargeId,
    string? FailureMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt);
