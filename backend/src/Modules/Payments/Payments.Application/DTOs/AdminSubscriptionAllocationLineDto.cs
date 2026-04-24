namespace Payments.Application.DTOs;

public record AdminSubscriptionAllocationLineDto(
    Guid Id,
    string TeacherId,
    string TeacherName,
    Guid CourseId,
    string CourseTitle,
    decimal AllocationWeight,
    decimal ProgressPercent,
    int CompletedLessons,
    int TotalLessons,
    decimal GrossAmount,
    decimal PlatformCommissionAmount,
    decimal ProviderFeeAmount,
    decimal NetAmount,
    string Currency);
