namespace Payments.Application.DTOs;

public record TeacherSettlementDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string StudentName,
    decimal GrossAmount,
    decimal ProviderFeeAmount,
    decimal PlatformCommissionAmount,
    decimal NetAmount,
    decimal RefundedGrossAmount,
    decimal RefundedNetAmount,
    decimal DisputedGrossAmount,
    decimal DisputedNetAmount,
    string Currency,
    string Status,
    DateTime AvailableAt,
    DateTime? PaidOutAt,
    DateTime CreatedAt);
