namespace Payments.Application.DTOs;

public record TeacherSettlementSummaryDto(
    decimal TotalGrossAmount,
    decimal TotalNetAmount,
    decimal PendingNetAmount,
    decimal ReadyForPayoutNetAmount,
    decimal InPayoutNetAmount,
    decimal PaidOutNetAmount,
    decimal RefundedNetAmount,
    decimal DisputedNetAmount,
    int SettlementsCount,
    int SubscriptionAllocationCount,
    string Currency);
