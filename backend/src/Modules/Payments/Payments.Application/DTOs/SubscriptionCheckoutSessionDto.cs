namespace Payments.Application.DTOs;

public record SubscriptionCheckoutSessionDto(
    Guid SubscriptionPaymentAttemptId,
    string CheckoutUrl);
