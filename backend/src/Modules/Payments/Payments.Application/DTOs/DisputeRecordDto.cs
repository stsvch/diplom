namespace Payments.Application.DTOs;

public record DisputeRecordDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    decimal TeacherNetDisputeAmount,
    string Currency,
    string Status,
    string? Reason,
    DateTime OpenedAt,
    DateTime? EvidenceDueBy,
    DateTime? FundsWithdrawnAt,
    DateTime? FundsReinstatedAt,
    DateTime? ClosedAt);
