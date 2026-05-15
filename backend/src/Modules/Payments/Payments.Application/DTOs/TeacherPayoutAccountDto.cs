// TeacherPayoutAccountDto.cs
namespace Payments.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public record TeacherPayoutAccountDto(
    string Status,
    bool ProviderConfigured,
    bool ChargesEnabled,
    bool PayoutsEnabled,
    bool DetailsSubmitted,
    bool CanPublishPaidCourses,
    string? RequirementsSummary);
