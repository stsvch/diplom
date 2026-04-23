using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.UpdatePlatformSettings;

public record UpdatePlatformSettingsCommand(
    bool RegistrationOpen,
    bool MaintenanceMode,
    string PlatformName,
    string SupportEmail
) : IRequest<Result<PlatformSettingsDto>>;
