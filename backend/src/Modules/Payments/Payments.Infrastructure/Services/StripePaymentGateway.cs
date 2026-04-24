using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payments.Application.Interfaces;
using Payments.Infrastructure.Configuration;

namespace Payments.Infrastructure.Services;

public class StripePaymentGateway : IPaymentProviderGateway
{
    private const string StripeApiBaseUrl = "https://api.stripe.com";

    private readonly HttpClient _httpClient;
    private readonly StripeOptions _stripeOptions;

    public StripePaymentGateway(HttpClient httpClient, IOptions<StripeOptions> stripeOptions)
    {
        _httpClient = httpClient;
        _stripeOptions = stripeOptions.Value;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_stripeOptions.SecretKey)
        && !string.IsNullOrWhiteSpace(_stripeOptions.WebhookSecret);

    public async Task<ProviderTeacherAccountResult> CreateTeacherAccountAsync(
        string teacherId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var fields = new Dictionary<string, string?>
        {
            ["type"] = "express",
            ["country"] = _stripeOptions.Country,
            ["email"] = email,
            ["business_type"] = "individual",
            ["capabilities[card_payments][requested]"] = "true",
            ["capabilities[transfers][requested]"] = "true",
            ["metadata[teacherId]"] = teacherId,
            ["metadata[displayName]"] = displayName,
        };

        var json = await SendFormAsync("/v1/accounts", fields, cancellationToken);
        return ParseTeacherAccount(json);
    }

    public async Task<ProviderTeacherAccountResult> GetTeacherAccountAsync(
        string providerAccountId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var json = await SendGetAsync($"/v1/accounts/{providerAccountId}", cancellationToken);
        return ParseTeacherAccount(json);
    }

    public async Task<string> CreateTeacherOnboardingLinkAsync(
        string providerAccountId,
        string refreshUrl,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var fields = new Dictionary<string, string?>
        {
            ["account"] = providerAccountId,
            ["refresh_url"] = refreshUrl,
            ["return_url"] = returnUrl,
            ["type"] = "account_onboarding",
        };

        var json = await SendFormAsync("/v1/account_links", fields, cancellationToken);
        return json.RootElement.GetProperty("url").GetString()
            ?? throw new InvalidOperationException("Stripe не вернул onboarding URL.");
    }

    public async Task<string> CreateTeacherDashboardLinkAsync(
        string providerAccountId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var json = await SendFormAsync(
            $"/v1/accounts/{providerAccountId}/login_links",
            [],
            cancellationToken);

        return json.RootElement.GetProperty("url").GetString()
            ?? throw new InvalidOperationException("Stripe не вернул dashboard URL.");
    }

    public async Task<string> CreateCustomerAsync(
        string userId,
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var fields = new Dictionary<string, string?>
        {
            ["email"] = email,
            ["name"] = displayName,
            ["metadata[userId]"] = userId,
        };

        var json = await SendFormAsync("/v1/customers", fields, cancellationToken);
        return json.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Stripe не вернул customer id.");
    }

    public async Task<ProviderCheckoutSessionResult> CreateCourseCheckoutSessionAsync(
        ProviderCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var amountInMinorUnits = ToMinorUnits(request.Amount);
        var fields = new Dictionary<string, string?>
        {
            ["mode"] = "payment",
            ["customer"] = request.ProviderCustomerId,
            ["success_url"] = request.SuccessUrl,
            ["cancel_url"] = request.CancelUrl,
            ["line_items[0][quantity]"] = "1",
            ["line_items[0][price_data][currency]"] = request.Currency,
            ["line_items[0][price_data][unit_amount]"] = amountInMinorUnits.ToString(CultureInfo.InvariantCulture),
            ["line_items[0][price_data][product_data][name]"] = request.CourseTitle,
            ["metadata[paymentAttemptId]"] = request.PaymentAttemptId.ToString(),
            ["metadata[courseId]"] = request.CourseId.ToString(),
            ["metadata[teacherId]"] = request.TeacherId,
            ["metadata[studentId]"] = request.StudentId,
            ["payment_intent_data[metadata][paymentAttemptId]"] = request.PaymentAttemptId.ToString(),
            ["payment_intent_data[metadata][courseId]"] = request.CourseId.ToString(),
            ["payment_intent_data[metadata][teacherId]"] = request.TeacherId,
            ["payment_intent_data[metadata][studentId]"] = request.StudentId,
        };

        if (request.SavePaymentMethodRequested)
            fields["saved_payment_method_options[payment_method_save]"] = "enabled";

        var json = await SendFormAsync("/v1/checkout/sessions", fields, cancellationToken);

        return new ProviderCheckoutSessionResult(
            json.RootElement.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Stripe не вернул session id."),
            json.RootElement.GetProperty("url").GetString()
                ?? throw new InvalidOperationException("Stripe не вернул checkout url."));
    }

    public async Task<ProviderCheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(
        ProviderSubscriptionCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var amountInMinorUnits = ToMinorUnits(request.Amount);
        var interval = request.BillingInterval.Trim().ToLowerInvariant() switch
        {
            "year" => "year",
            _ => "month",
        };

        var fields = new Dictionary<string, string?>
        {
            ["mode"] = "subscription",
            ["customer"] = request.ProviderCustomerId,
            ["success_url"] = request.SuccessUrl,
            ["cancel_url"] = request.CancelUrl,
            ["line_items[0][quantity]"] = "1",
            ["line_items[0][price_data][currency]"] = request.Currency,
            ["line_items[0][price_data][unit_amount]"] = amountInMinorUnits.ToString(CultureInfo.InvariantCulture),
            ["line_items[0][price_data][product_data][name]"] = request.PlanName,
            ["line_items[0][price_data][recurring][interval]"] = interval,
            ["line_items[0][price_data][recurring][interval_count]"] = request.BillingIntervalCount.ToString(CultureInfo.InvariantCulture),
            ["metadata[subscriptionPaymentAttemptId]"] = request.SubscriptionPaymentAttemptId.ToString(),
            ["metadata[subscriptionPlanId]"] = request.SubscriptionPlanId.ToString(),
            ["metadata[studentId]"] = request.StudentId,
            ["metadata[checkoutType]"] = "subscription",
            ["subscription_data[metadata][subscriptionPaymentAttemptId]"] = request.SubscriptionPaymentAttemptId.ToString(),
            ["subscription_data[metadata][subscriptionPlanId]"] = request.SubscriptionPlanId.ToString(),
            ["subscription_data[metadata][studentId]"] = request.StudentId,
        };

        var json = await SendFormAsync("/v1/checkout/sessions", fields, cancellationToken);

        return new ProviderCheckoutSessionResult(
            json.RootElement.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Stripe не вернул session id."),
            json.RootElement.GetProperty("url").GetString()
                ?? throw new InvalidOperationException("Stripe не вернул checkout url."));
    }

    public async Task<ProviderRefundResult> CreateRefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var fields = new Dictionary<string, string?>
        {
            ["payment_intent"] = request.PaymentIntentId,
            ["amount"] = ToMinorUnits(request.Amount).ToString(CultureInfo.InvariantCulture),
            ["metadata[paymentAttemptId]"] = request.PaymentAttemptId.ToString(),
            ["metadata[courseId]"] = request.CourseId.ToString(),
            ["metadata[teacherId]"] = request.TeacherId,
            ["metadata[studentId]"] = request.StudentId,
            ["metadata[adminId]"] = request.RequestedByAdminId,
            ["metadata[internalReason]"] = request.Reason,
        };

        if (request.Reason is "duplicate" or "fraudulent" or "requested_by_customer")
            fields["reason"] = request.Reason;

        var json = await SendFormAsync("/v1/refunds", fields, cancellationToken);
        var root = json.RootElement;

        return new ProviderRefundResult(
            root.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Stripe не вернул refund id."),
            root.TryGetProperty("payment_intent", out var paymentIntentElement)
                ? paymentIntentElement.GetString() ?? request.PaymentIntentId
                : request.PaymentIntentId,
            root.TryGetProperty("amount", out var amountElement)
                ? FromMinorUnits(amountElement.GetInt64())
                : request.Amount,
            root.TryGetProperty("currency", out var currencyElement)
                ? currencyElement.GetString() ?? request.Currency
                : request.Currency,
            root.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null,
            root.TryGetProperty("reason", out var reasonElement) ? reasonElement.GetString() : request.Reason,
            root.TryGetProperty("failure_reason", out var failureReasonElement) ? failureReasonElement.GetString() : null);
    }

    public async Task<ProviderTransferResult> CreateTransferAsync(
        ProviderTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var fields = new Dictionary<string, string?>
        {
            ["amount"] = ToMinorUnits(request.Amount).ToString(CultureInfo.InvariantCulture),
            ["currency"] = request.Currency,
            ["destination"] = request.ProviderAccountId,
            ["transfer_group"] = request.PayoutRecordId.ToString(),
            ["metadata[payoutRecordId]"] = request.PayoutRecordId.ToString(),
            ["metadata[teacherId]"] = request.TeacherId,
            ["metadata[settlementsCount]"] = request.SettlementsCount.ToString(CultureInfo.InvariantCulture),
        };

        var json = await SendFormAsync("/v1/transfers", fields, cancellationToken);
        var root = json.RootElement;

        return new ProviderTransferResult(
            root.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Stripe не вернул transfer id."),
            root.TryGetProperty("amount", out var amountElement)
                ? FromMinorUnits(amountElement.GetInt64())
                : request.Amount,
            root.TryGetProperty("currency", out var currencyElement)
                ? currencyElement.GetString() ?? request.Currency
                : request.Currency);
    }

    public StripeWebhookEvent ParseWebhook(string payload, string? signatureHeader)
    {
        EnsureConfigured();
        VerifyStripeSignature(payload, signatureHeader);

        using var json = JsonDocument.Parse(payload);
        var root = json.RootElement;
        var eventId = root.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Некорректный Stripe webhook: отсутствует id.");
        var eventType = root.GetProperty("type").GetString()
            ?? throw new InvalidOperationException("Некорректный Stripe webhook: отсутствует type.");
        var dataObject = root.GetProperty("data").GetProperty("object");
        var metadata = dataObject.TryGetProperty("metadata", out var metadataElement)
            ? ReadStringDictionary(metadataElement)
            : new Dictionary<string, string>();
        if (eventType.StartsWith("invoice.", StringComparison.OrdinalIgnoreCase))
        {
            if (dataObject.TryGetProperty("subscription_details", out var subscriptionDetailsElement)
                && subscriptionDetailsElement.ValueKind == JsonValueKind.Object
                && subscriptionDetailsElement.TryGetProperty("metadata", out var subscriptionMetadataElement))
            {
                MergeMetadata(metadata, ReadStringDictionary(subscriptionMetadataElement));
            }

            if (dataObject.TryGetProperty("parent", out var parentElement)
                && parentElement.ValueKind == JsonValueKind.Object
                && parentElement.TryGetProperty("subscription_details", out var parentSubscriptionDetailsElement)
                && parentSubscriptionDetailsElement.ValueKind == JsonValueKind.Object
                && parentSubscriptionDetailsElement.TryGetProperty("metadata", out var parentSubscriptionMetadataElement))
            {
                MergeMetadata(metadata, ReadStringDictionary(parentSubscriptionMetadataElement));
            }
        }

        string? requirementsSummary = null;
        if (dataObject.TryGetProperty("requirements", out var requirementsElement))
            requirementsSummary = BuildRequirementsSummary(requirementsElement);

        string? failureMessage = null;
        if (dataObject.TryGetProperty("last_payment_error", out var errorElement)
            && errorElement.TryGetProperty("message", out var messageElement))
        {
            failureMessage = messageElement.GetString();
        }
        else if (dataObject.TryGetProperty("failure_reason", out var failureReasonElement))
        {
            failureMessage = failureReasonElement.GetString();
        }

        var objectId = dataObject.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        var status = dataObject.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null;
        var reason = dataObject.TryGetProperty("reason", out var reasonElement) ? reasonElement.GetString() : null;
        var providerSubscriptionId = eventType.StartsWith("customer.subscription", StringComparison.OrdinalIgnoreCase)
            ? objectId
            : dataObject.TryGetProperty("subscription", out var subscriptionElement)
                ? ReadStringOrNestedId(subscriptionElement)
                : null;
        var disputeEvidenceDueBy = dataObject.TryGetProperty("evidence_details", out var evidenceDetailsElement)
            && evidenceDetailsElement.TryGetProperty("due_by", out var dueByElement)
                ? ReadUnixTimestamp(dueByElement)
                : null;
        var currentPeriodStart = dataObject.TryGetProperty("current_period_start", out var currentPeriodStartElement)
            ? ReadUnixTimestamp(currentPeriodStartElement)
            : null;
        var currentPeriodEnd = dataObject.TryGetProperty("current_period_end", out var currentPeriodEndElement)
            ? ReadUnixTimestamp(currentPeriodEndElement)
            : null;
        if ((currentPeriodStart == null || currentPeriodEnd == null)
            && TryReadInvoiceLinePeriod(dataObject, out var invoicePeriodStart, out var invoicePeriodEnd))
        {
            currentPeriodStart ??= invoicePeriodStart;
            currentPeriodEnd ??= invoicePeriodEnd;
        }
        var cancelAtPeriodEnd = dataObject.TryGetProperty("cancel_at_period_end", out var cancelAtPeriodEndElement)
            ? (bool?)cancelAtPeriodEndElement.GetBoolean()
            : null;
        var subscriptionCanceledAt = dataObject.TryGetProperty("canceled_at", out var canceledAtElement)
            ? ReadUnixTimestamp(canceledAtElement)
            : null;
        var invoiceStatus = eventType.StartsWith("invoice.", StringComparison.OrdinalIgnoreCase) ? status : null;
        var invoiceBillingReason = eventType.StartsWith("invoice.", StringComparison.OrdinalIgnoreCase)
            && dataObject.TryGetProperty("billing_reason", out var billingReasonElement)
                ? billingReasonElement.GetString()
                : null;
        var invoiceDueDate = dataObject.TryGetProperty("due_date", out var dueDateElement)
            ? ReadUnixTimestamp(dueDateElement)
            : null;
        var invoicePaidAt = dataObject.TryGetProperty("status_transitions", out var statusTransitionsElement)
            && statusTransitionsElement.ValueKind == JsonValueKind.Object
            && statusTransitionsElement.TryGetProperty("paid_at", out var paidAtElement)
                ? ReadUnixTimestamp(paidAtElement)
                : null;
        long? amountMinor = dataObject.TryGetProperty("amount", out var amountElement) ? amountElement.GetInt64() : null;
        long? amountDueMinor = dataObject.TryGetProperty("amount_due", out var amountDueElement) ? amountDueElement.GetInt64() : null;
        long? amountPaidMinor = dataObject.TryGetProperty("amount_paid", out var amountPaidElement) ? amountPaidElement.GetInt64() : null;

        return new StripeWebhookEvent(
            eventId,
            eventType,
            eventType == "account.updated" ? objectId : null,
            eventType.StartsWith("transfer.", StringComparison.OrdinalIgnoreCase)
                ? eventType.Equals("transfer.reversed", StringComparison.OrdinalIgnoreCase)
                    ? dataObject.TryGetProperty("transfer", out var transferElement)
                        ? ReadStringOrNestedId(transferElement)
                        : metadata.TryGetValue("providerTransferId", out var providerTransferId)
                            ? providerTransferId
                            : null
                    : objectId
                : null,
            eventType.StartsWith("refund.") ? objectId : null,
            eventType.StartsWith("charge.dispute.") ? objectId : null,
            eventType.StartsWith("invoice.", StringComparison.OrdinalIgnoreCase)
                ? objectId
                : dataObject.TryGetProperty("invoice", out var invoiceElement)
                    ? ReadStringOrNestedId(invoiceElement)
                    : null,
            eventType.StartsWith("checkout.session") ? objectId : null,
            providerSubscriptionId,
            dataObject.TryGetProperty("payment_intent", out var paymentIntentElement)
                ? ReadStringOrNestedId(paymentIntentElement)
                : eventType.StartsWith("payment_intent")
                    ? objectId
                    : null,
            dataObject.TryGetProperty("customer", out var customerElement) ? customerElement.GetString() : null,
            eventType.StartsWith("customer.subscription", StringComparison.OrdinalIgnoreCase) ? status : null,
            invoiceStatus,
            invoiceBillingReason,
            dataObject.TryGetProperty("payment_status", out var paymentStatusElement) ? paymentStatusElement.GetString() : null,
            failureMessage,
            eventType.StartsWith("refund.") ? status : null,
            eventType.StartsWith("refund.") ? reason : null,
            eventType.StartsWith("charge.dispute.") ? status : null,
            eventType.StartsWith("charge.dispute.") ? reason : null,
            disputeEvidenceDueBy,
            currentPeriodStart,
            currentPeriodEnd,
            cancelAtPeriodEnd,
            subscriptionCanceledAt,
            invoiceDueDate,
            invoicePaidAt,
            amountMinor,
            amountDueMinor,
            amountPaidMinor,
            dataObject.TryGetProperty("currency", out var currencyElement) ? currencyElement.GetString() : null,
            metadata,
            dataObject.TryGetProperty("charges_enabled", out var chargesEnabledElement) ? chargesEnabledElement.GetBoolean() : null,
            dataObject.TryGetProperty("payouts_enabled", out var payoutsEnabledElement) ? payoutsEnabledElement.GetBoolean() : null,
            dataObject.TryGetProperty("details_submitted", out var detailsSubmittedElement) ? detailsSubmittedElement.GetBoolean() : null,
            requirementsSummary);
    }

    public async Task<ProviderChargeSnapshot?> GetPaymentChargeSnapshotAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var json = await SendGetAsync(
            $"/v1/payment_intents/{paymentIntentId}?expand[]=latest_charge.balance_transaction",
            cancellationToken);

        if (!json.RootElement.TryGetProperty("latest_charge", out var latestChargeElement)
            || latestChargeElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var chargeId = latestChargeElement.TryGetProperty("id", out var chargeIdElement)
            ? chargeIdElement.GetString()
            : null;

        decimal providerFeeAmount = 0m;
        if (latestChargeElement.TryGetProperty("balance_transaction", out var balanceTransactionElement)
            && balanceTransactionElement.ValueKind == JsonValueKind.Object
            && balanceTransactionElement.TryGetProperty("fee", out var feeElement))
        {
            providerFeeAmount = FromMinorUnits(feeElement.GetInt64());
        }

        if (string.IsNullOrWhiteSpace(chargeId))
            return null;

        return new ProviderChargeSnapshot(chargeId, providerFeeAmount);
    }

    public async Task<ProviderPaymentMethodSnapshot?> GetPaymentMethodSnapshotAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var json = await SendGetAsync(
            $"/v1/payment_intents/{paymentIntentId}?expand[]=payment_method",
            cancellationToken);

        if (!json.RootElement.TryGetProperty("payment_method", out var paymentMethodElement)
            || paymentMethodElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var paymentMethodId = paymentMethodElement.GetProperty("id").GetString();
        var customerId = paymentMethodElement.TryGetProperty("customer", out var customerElement)
            ? customerElement.GetString()
            : null;

        string? brand = null;
        string? last4 = null;
        int? expMonth = null;
        int? expYear = null;
        string? allowRedisplay = null;

        if (paymentMethodElement.TryGetProperty("card", out var cardElement))
        {
            brand = cardElement.TryGetProperty("brand", out var brandElement) ? brandElement.GetString() : null;
            last4 = cardElement.TryGetProperty("last4", out var last4Element) ? last4Element.GetString() : null;
            expMonth = cardElement.TryGetProperty("exp_month", out var expMonthElement) ? expMonthElement.GetInt32() : null;
            expYear = cardElement.TryGetProperty("exp_year", out var expYearElement) ? expYearElement.GetInt32() : null;
        }

        if (paymentMethodElement.TryGetProperty("allow_redisplay", out var allowRedisplayElement))
            allowRedisplay = allowRedisplayElement.GetString();

        if (string.IsNullOrWhiteSpace(paymentMethodId) || string.IsNullOrWhiteSpace(customerId))
            return null;

        if (string.Equals(allowRedisplay, "limited", StringComparison.OrdinalIgnoreCase))
            return null;

        return new ProviderPaymentMethodSnapshot(
            customerId,
            paymentMethodId,
            brand,
            last4,
            expMonth,
            expYear);
    }

    public async Task DetachPaymentMethodAsync(
        string providerPaymentMethodId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        await SendFormAsync(
            $"/v1/payment_methods/{providerPaymentMethodId}/detach",
            [],
            cancellationToken);
    }

    private async Task<JsonDocument> SendFormAsync(
        string relativeUrl,
        Dictionary<string, string?> fields,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{StripeApiBaseUrl}{relativeUrl}")
        {
            Content = new FormUrlEncodedContent(
                fields
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                    .Select(x => new KeyValuePair<string, string>(x.Key, x.Value!)))
        };

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _stripeOptions.SecretKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Stripe error: {payload}");

        return JsonDocument.Parse(payload);
    }

    private async Task<JsonDocument> SendGetAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{StripeApiBaseUrl}{relativeUrl}");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _stripeOptions.SecretKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Stripe error: {payload}");

        return JsonDocument.Parse(payload);
    }

    private static ProviderTeacherAccountResult ParseTeacherAccount(JsonDocument json)
    {
        var root = json.RootElement;
        return new ProviderTeacherAccountResult(
            root.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Stripe не вернул account id."),
            root.TryGetProperty("charges_enabled", out var chargesEnabledElement) && chargesEnabledElement.GetBoolean(),
            root.TryGetProperty("payouts_enabled", out var payoutsEnabledElement) && payoutsEnabledElement.GetBoolean(),
            root.TryGetProperty("details_submitted", out var detailsSubmittedElement) && detailsSubmittedElement.GetBoolean(),
            root.TryGetProperty("requirements", out var requirementsElement)
                ? BuildRequirementsSummary(requirementsElement)
                : null);
    }

    private void VerifyStripeSignature(string payload, string? signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
            throw new InvalidOperationException("Отсутствует Stripe-Signature.");

        var parts = signatureHeader
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .Where(part => part.Length == 2)
            .ToDictionary(part => part[0], part => part[1], StringComparer.OrdinalIgnoreCase);

        if (!parts.TryGetValue("t", out var timestamp) || !parts.TryGetValue("v1", out var signature))
            throw new InvalidOperationException("Некорректный Stripe-Signature.");

        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_stripeOptions.WebhookSecret));
        var computedSignature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload))).ToLowerInvariant();

        var expectedBytes = Encoding.UTF8.GetBytes(signature);
        var actualBytes = Encoding.UTF8.GetBytes(computedSignature);
        if (expectedBytes.Length != actualBytes.Length
            || !CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes))
        {
            throw new InvalidOperationException("Подпись Stripe webhook не прошла проверку.");
        }
    }

    private static Dictionary<string, string> ReadStringDictionary(JsonElement element)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (element.ValueKind != JsonValueKind.Object)
            return result;

        foreach (var property in element.EnumerateObject())
            result[property.Name] = property.Value.GetString() ?? string.Empty;

        return result;
    }

    private static string? BuildRequirementsSummary(JsonElement requirementsElement)
    {
        var items = new List<string>();

        if (requirementsElement.TryGetProperty("currently_due", out var currentlyDue)
            && currentlyDue.ValueKind == JsonValueKind.Array)
        {
            items.AddRange(currentlyDue.EnumerateArray()
                .Select(x => x.GetString())
                .Where(x => !string.IsNullOrWhiteSpace(x))!);
        }

        if (requirementsElement.TryGetProperty("disabled_reason", out var disabledReasonElement))
        {
            var disabledReason = disabledReasonElement.GetString();
            if (!string.IsNullOrWhiteSpace(disabledReason))
                items.Add($"disabled_reason: {disabledReason}");
        }

        if (items.Count == 0)
            return null;

        return string.Join(", ", items.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static void MergeMetadata(Dictionary<string, string> target, IReadOnlyDictionary<string, string> source)
    {
        foreach (var pair in source)
        {
            if (!target.ContainsKey(pair.Key))
                target[pair.Key] = pair.Value;
        }
    }

    private static string? ReadStringOrNestedId(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Object when element.TryGetProperty("id", out var idElement) => idElement.GetString(),
            _ => null,
        };
    }

    private static bool TryReadInvoiceLinePeriod(
        JsonElement invoiceElement,
        out DateTime? periodStart,
        out DateTime? periodEnd)
    {
        periodStart = null;
        periodEnd = null;

        if (!invoiceElement.TryGetProperty("lines", out var linesElement)
            || linesElement.ValueKind != JsonValueKind.Object
            || !linesElement.TryGetProperty("data", out var dataElement)
            || dataElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var lineElement in dataElement.EnumerateArray())
        {
            if (lineElement.ValueKind != JsonValueKind.Object
                || !lineElement.TryGetProperty("period", out var periodElement)
                || periodElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (periodElement.TryGetProperty("start", out var startElement))
                periodStart = ReadUnixTimestamp(startElement);

            if (periodElement.TryGetProperty("end", out var endElement))
                periodEnd = ReadUnixTimestamp(endElement);

            if (periodStart != null || periodEnd != null)
                return true;
        }

        return false;
    }

    private static DateTime? ReadUnixTimestamp(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
            return null;

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var unixSeconds) && unixSeconds > 0)
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;

        return null;
    }

    private static long ToMinorUnits(decimal amount)
    {
        return decimal.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
    }

    private static decimal FromMinorUnits(long amountMinor)
    {
        return decimal.Round(amountMinor / 100m, 2, MidpointRounding.AwayFromZero);
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Stripe не настроен.");
    }
}
