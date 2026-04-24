namespace Payments.Application.DTOs;

public record PayoutRecordDto(
    Guid Id,
    decimal Amount,
    string Currency,
    int SettlementsCount,
    int AllocationLinesCount,
    string Status,
    string? ProviderTransferId,
    DateTime RequestedAt,
    DateTime? SubmittedAt,
    DateTime? PaidAt,
    DateTime? FailedAt,
    string? FailureMessage);
