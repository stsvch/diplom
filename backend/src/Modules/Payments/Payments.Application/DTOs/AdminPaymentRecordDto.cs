// AdminPaymentRecordDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public record AdminPaymentRecordDto(
    Guid PaymentAttemptId,
    Guid CourseId,
    string CourseTitle,
    string StudentId,
    string StudentName,
    string TeacherId,
    string TeacherName,
    decimal Amount,
    decimal ProviderFeeAmount,
    string Currency,
    string PaymentStatus,
    string? ProviderChargeId,
    string? PurchaseStatus,
    DateTime CreatedAt,
    DateTime? CompletedAt);
