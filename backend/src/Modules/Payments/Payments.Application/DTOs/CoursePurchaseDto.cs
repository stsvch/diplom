namespace Payments.Application.DTOs;

public record CoursePurchaseDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PurchasedAt);
