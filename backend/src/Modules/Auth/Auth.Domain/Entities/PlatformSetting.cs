// PlatformSetting.cs
namespace Auth.Domain.Entities;

// Singleton-настройки платформы: запись с Id=1 управляет регистрацией, режимом обслуживания, названием платформы и email поддержки.
public class PlatformSetting
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;
    public bool RegistrationOpen { get; set; } = true;
    public bool MaintenanceMode { get; set; } = false;
    public string PlatformName { get; set; } = "EduPlatform";
    public string SupportEmail { get; set; } = "support@eduplatform.local";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
