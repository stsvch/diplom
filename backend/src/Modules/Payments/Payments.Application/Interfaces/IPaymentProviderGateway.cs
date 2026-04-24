namespace Payments.Application.Interfaces;

public interface IPaymentProviderGateway
{
    bool IsConfigured { get; }

    Task<ProviderTeacherAccountResult> CreateTeacherAccountAsync(
        string teacherId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default);

    Task<ProviderTeacherAccountResult> GetTeacherAccountAsync(
        string providerAccountId,
        CancellationToken cancellationToken = default);

    Task<string> CreateTeacherOnboardingLinkAsync(
        string providerAccountId,
        string refreshUrl,
        string returnUrl,
        CancellationToken cancellationToken = default);

    Task<string> CreateTeacherDashboardLinkAsync(
        string providerAccountId,
        CancellationToken cancellationToken = default);

    Task<string> CreateCustomerAsync(
        string userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default);

    Task<ProviderCheckoutSessionResult> CreateCourseCheckoutSessionAsync(
        ProviderCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ProviderCheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(
        ProviderSubscriptionCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ProviderRefundResult> CreateRefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken = default);

    Task<ProviderTransferResult> CreateTransferAsync(
        ProviderTransferRequest request,
        CancellationToken cancellationToken = default);

    StripeWebhookEvent ParseWebhook(string payload, string? signatureHeader);

    Task<ProviderChargeSnapshot?> GetPaymentChargeSnapshotAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    Task<ProviderPaymentMethodSnapshot?> GetPaymentMethodSnapshotAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    Task DetachPaymentMethodAsync(
        string providerPaymentMethodId,
        CancellationToken cancellationToken = default);
}

public record ProviderTeacherAccountResult(
    string ProviderAccountId,
    bool ChargesEnabled,
    bool PayoutsEnabled,
    bool DetailsSubmitted,
    string? RequirementsSummary);

public record ProviderCheckoutSessionRequest(
    string ProviderCustomerId,
    string Currency,
    decimal Amount,
    string CourseTitle,
    Guid PaymentAttemptId,
    Guid CourseId,
    string TeacherId,
    string StudentId,
    string SuccessUrl,
    string CancelUrl,
    bool SavePaymentMethodRequested);

public record ProviderSubscriptionCheckoutSessionRequest(
    string ProviderCustomerId,
    string Currency,
    decimal Amount,
    string PlanName,
    Guid SubscriptionPaymentAttemptId,
    Guid SubscriptionPlanId,
    string StudentId,
    string SuccessUrl,
    string CancelUrl,
    string BillingInterval,
    int BillingIntervalCount);

public record ProviderCheckoutSessionResult(
    string SessionId,
    string CheckoutUrl);

public record ProviderRefundRequest(
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    string? Reason,
    Guid PaymentAttemptId,
    Guid CourseId,
    string TeacherId,
    string StudentId,
    string? RequestedByAdminId);

public record ProviderRefundResult(
    string ProviderRefundId,
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    string? Status,
    string? Reason,
    string? FailureMessage);

public record ProviderTransferRequest(
    Guid PayoutRecordId,
    string TeacherId,
    string ProviderAccountId,
    decimal Amount,
    string Currency,
    int SettlementsCount);

public record ProviderTransferResult(
    string ProviderTransferId,
    decimal Amount,
    string Currency);

public record ProviderPaymentMethodSnapshot(
    string ProviderCustomerId,
    string ProviderPaymentMethodId,
    string? Brand,
    string? Last4,
    int? ExpMonth,
    int? ExpYear);

public record ProviderChargeSnapshot(
    string ProviderChargeId,
    decimal ProviderFeeAmount);

public record StripeWebhookEvent(
    string EventId,
    string EventType,
    string? ProviderAccountId,
    string? ProviderTransferId,
    string? ProviderRefundId,
    string? ProviderDisputeId,
    string? ProviderInvoiceId,
    string? SessionId,
    string? ProviderSubscriptionId,
    string? PaymentIntentId,
    string? CustomerId,
    string? SubscriptionStatus,
    string? InvoiceStatus,
    string? InvoiceBillingReason,
    string? PaymentStatus,
    string? FailureMessage,
    string? RefundStatus,
    string? RefundReason,
    string? DisputeStatus,
    string? DisputeReason,
    DateTime? DisputeEvidenceDueBy,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    bool? CancelAtPeriodEnd,
    DateTime? SubscriptionCanceledAt,
    DateTime? InvoiceDueDate,
    DateTime? InvoicePaidAt,
    long? AmountMinor,
    long? AmountDueMinor,
    long? AmountPaidMinor,
    string? Currency,
    Dictionary<string, string> Metadata,
    bool? ChargesEnabled,
    bool? PayoutsEnabled,
    bool? DetailsSubmitted,
    string? RequirementsSummary);
