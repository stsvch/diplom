namespace Payments.Application.DTOs;

public record SubscriptionInvoiceDto(
    Guid Id,
    Guid SubscriptionPlanId,
    string PlanName,
    decimal AmountDue,
    decimal AmountPaid,
    string Currency,
    string Status,
    string? BillingReason,
    DateTime? PeriodStart,
    DateTime? PeriodEnd,
    DateTime? DueDate,
    DateTime? PaidAt,
    string? FailureMessage,
    DateTime CreatedAt);
