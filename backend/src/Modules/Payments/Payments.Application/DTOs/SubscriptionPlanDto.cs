namespace Payments.Application.DTOs;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    string BillingInterval,
    int BillingIntervalCount,
    bool IsActive,
    bool IsFeatured,
    int SortOrder,
    string? ProviderProductId,
    string? ProviderPriceId,
    int IndividualSlotsPerMonth,
    int GroupSlotsPerMonth,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
