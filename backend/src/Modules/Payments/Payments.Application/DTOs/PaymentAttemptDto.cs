namespace Payments.Application.DTOs;

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
