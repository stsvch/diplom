namespace Payments.Application.DTOs;

public record AdminPaymentRecordDto(
    Guid PaymentAttemptId,
    Guid CourseId,
    string CourseTitle,
    string StudentId,
    string StudentName,
    string TeacherId,
    string TeacherName,
    decimal Amount,
    decimal RefundedAmount,
    decimal PendingRefundAmount,
    decimal DisputedAmount,
    decimal RemainingRefundableAmount,
    decimal ProviderFeeAmount,
    string Currency,
    string PaymentStatus,
    string? ProviderChargeId,
    string? LatestDisputeStatus,
    string? PurchaseStatus,
    DateTime CreatedAt,
    DateTime? CompletedAt);
