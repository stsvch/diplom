namespace Payments.Application.DTOs;

public record TeacherPayoutAccountDto(
    string Status,
    bool ProviderConfigured,
    bool ChargesEnabled,
    bool PayoutsEnabled,
    bool DetailsSubmitted,
    bool CanPublishPaidCourses,
    string? RequirementsSummary);
