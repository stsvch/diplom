// CoursePurchaseDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public record CoursePurchaseDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PurchasedAt);
