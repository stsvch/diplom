namespace Auth.Application.DTOs;

public class PlatformSettingsDto
{
    public bool RegistrationOpen { get; set; } = true;
    public bool MaintenanceMode { get; set; } = false;
    public string PlatformName { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
}
