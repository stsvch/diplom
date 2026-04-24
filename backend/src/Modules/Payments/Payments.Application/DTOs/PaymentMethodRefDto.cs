namespace Payments.Application.DTOs;

public record PaymentMethodRefDto(
    Guid Id,
    string? Brand,
    string? Last4,
    int? ExpMonth,
    int? ExpYear,
    bool IsDefault);
