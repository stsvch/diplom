// TeacherSettlementDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public record TeacherSettlementDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string StudentName,
    decimal GrossAmount,
    decimal ProviderFeeAmount,
    decimal PlatformCommissionAmount,
    decimal NetAmount,
    string Currency,
    string Status,
    DateTime AvailableAt,
    DateTime? PaidOutAt,
    DateTime CreatedAt);
