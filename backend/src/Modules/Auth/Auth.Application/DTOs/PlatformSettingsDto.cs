// PlatformSettingsDto.cs
namespace Auth.Application.DTOs;

// Настройки платформы для UI: открыта ли регистрация, включён ли maintenance mode, какое имя платформы и email поддержки показывать.
public class PlatformSettingsDto
{
    public bool RegistrationOpen { get; set; } = true;
    public bool MaintenanceMode { get; set; } = false;
    public string PlatformName { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
}
