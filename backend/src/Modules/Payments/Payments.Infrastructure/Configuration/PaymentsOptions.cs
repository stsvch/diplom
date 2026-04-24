namespace Payments.Infrastructure.Configuration;

public class PaymentsOptions
{
    public string Provider { get; set; } = "Stripe";
    public string Currency { get; set; } = "usd";
    public int SettlementHoldDays { get; set; } = 7;
    public decimal PlatformCommissionPercent { get; set; }
}

public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string Country { get; set; } = "US";
}
