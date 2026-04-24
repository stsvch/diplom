namespace Payments.Application.DTOs;

public record RefundRecordDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    decimal TeacherNetRefundAmount,
    string Currency,
    string Status,
    string? Reason,
    string? FailureMessage,
    DateTime RequestedAt,
    DateTime? ProcessedAt);
