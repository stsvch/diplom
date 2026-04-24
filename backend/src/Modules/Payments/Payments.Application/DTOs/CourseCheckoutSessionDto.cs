namespace Payments.Application.DTOs;

public record CourseCheckoutSessionDto(
    Guid PaymentAttemptId,
    string CheckoutUrl);
