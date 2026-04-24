namespace Payments.Application.DTOs;

public record AdminSubscriptionAllocationRunDto(
    Guid Id,
    Guid SubscriptionInvoiceId,
    string StudentId,
    Guid SubscriptionPlanId,
    string PlanName,
    decimal GrossAmount,
    decimal PlatformCommissionAmount,
    decimal ProviderFeeAmount,
    decimal NetAmount,
    string Currency,
    string Strategy,
    string Status,
    int TeacherCount,
    int CourseCount,
    DateTime? PeriodStart,
    DateTime? PeriodEnd,
    DateTime AllocatedAt,
    IReadOnlyList<AdminSubscriptionAllocationLineDto> Lines);
