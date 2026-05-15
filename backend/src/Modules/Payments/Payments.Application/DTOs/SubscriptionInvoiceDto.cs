// SubscriptionInvoiceDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
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
