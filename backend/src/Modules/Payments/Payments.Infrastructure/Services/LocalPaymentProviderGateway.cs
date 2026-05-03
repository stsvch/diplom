using Payments.Application.Interfaces;

namespace Payments.Infrastructure.Services;

public class LocalPaymentProviderGateway : IPaymentProviderGateway
{
    public bool IsConfigured => true;

    public Task<ProviderTeacherAccountResult> CreateTeacherAccountAsync(
        string teacherId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateReadyTeacherAccount(teacherId));
    }

    public Task<ProviderTeacherAccountResult> GetTeacherAccountAsync(
        string providerAccountId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderTeacherAccountResult(
            providerAccountId,
            ChargesEnabled: true,
            PayoutsEnabled: true,
            DetailsSubmitted: true,
            RequirementsSummary: null));
    }

    public Task<string> CreateTeacherOnboardingLinkAsync(
        string providerAccountId,
        string refreshUrl,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AppendQuery(returnUrl, "localPayout", "connected"));
    }

    public Task<string> CreateTeacherDashboardLinkAsync(
        string providerAccountId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult("/teacher/payments?localPayoutDashboard=true");
    }

    public Task<string> CreateCustomerAsync(
        string userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"cus_local_{Normalize(userId)}");
    }

    public Task<ProviderCheckoutSessionResult> CreateCourseCheckoutSessionAsync(
        ProviderCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var checkoutUrl = AppendQuery(request.SuccessUrl, "localCheckout", "paid");
        return Task.FromResult(new ProviderCheckoutSessionResult(
            $"cs_local_{request.PaymentAttemptId:N}",
            checkoutUrl));
    }

    public Task<ProviderCheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(
        ProviderSubscriptionCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var checkoutUrl = AppendQuery(request.SuccessUrl, "localCheckout", "subscription_paid");
        return Task.FromResult(new ProviderCheckoutSessionResult(
            $"cs_sub_local_{request.SubscriptionPaymentAttemptId:N}",
            checkoutUrl));
    }

    public Task<ProviderRefundResult> CreateRefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderRefundResult(
            $"re_local_{request.PaymentAttemptId:N}",
            request.PaymentIntentId,
            request.Amount,
            request.Currency,
            "succeeded",
            request.Reason,
            null));
    }

    public Task<ProviderTransferResult> CreateTransferAsync(
        ProviderTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderTransferResult(
            $"tr_local_{request.PayoutRecordId:N}",
            request.Amount,
            request.Currency));
    }

    public StripeWebhookEvent ParseWebhook(string payload, string? signatureHeader)
    {
        throw new InvalidOperationException("Локальный платежный провайдер не принимает Stripe webhooks.");
    }

    public Task<ProviderChargeSnapshot?> GetPaymentChargeSnapshotAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ProviderChargeSnapshot?>(new ProviderChargeSnapshot(
            $"ch_local_{Normalize(paymentIntentId)}",
            ProviderFeeAmount: 0m));
    }

    public Task<ProviderPaymentMethodSnapshot?> GetPaymentMethodSnapshotAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ProviderPaymentMethodSnapshot?>(new ProviderPaymentMethodSnapshot(
            $"cus_local_saved_{Normalize(paymentIntentId)}",
            $"pm_local_{Normalize(paymentIntentId)}",
            "visa",
            "4242",
            12,
            2030));
    }

    public Task DetachPaymentMethodAsync(
        string providerPaymentMethodId,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private static ProviderTeacherAccountResult CreateReadyTeacherAccount(string teacherId)
    {
        return new ProviderTeacherAccountResult(
            $"acct_local_{Normalize(teacherId)}",
            ChargesEnabled: true,
            PayoutsEnabled: true,
            DetailsSubmitted: true,
            RequirementsSummary: null);
    }

    private static string AppendQuery(string url, string key, string value)
    {
        var separator = url.Contains('?') ? '&' : '?';
        return $"{url}{separator}{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
    }

    private static string Normalize(string value)
    {
        var normalized = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized)
            ? Guid.NewGuid().ToString("N")
            : normalized;
    }
}
