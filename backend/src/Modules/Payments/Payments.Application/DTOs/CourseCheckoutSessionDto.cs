// CourseCheckoutSessionDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public record CourseCheckoutSessionDto(
    Guid PaymentAttemptId,
    string CheckoutUrl);
