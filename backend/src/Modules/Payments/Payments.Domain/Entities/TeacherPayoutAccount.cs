using EduPlatform.Shared.Domain;
using Payments.Domain.Enums;

namespace Payments.Domain.Entities;

public class TeacherPayoutAccount : BaseEntity, IAuditableEntity
{
    public string TeacherId { get; set; } = string.Empty;
    public string Provider { get; set; } = "Stripe";
    public string ProviderAccountId { get; set; } = string.Empty;
    public TeacherPayoutAccountStatus Status { get; set; } = TeacherPayoutAccountStatus.NotStarted;
    public bool ChargesEnabled { get; set; }
    public bool PayoutsEnabled { get; set; }
    public bool DetailsSubmitted { get; set; }
    public string? RequirementsSummary { get; set; }
    public DateTime? OnboardingStartedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
