// UpdatePlatformSettingsCommand.cs
using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.UpdatePlatformSettings;

// Команда содержит публичные настройки платформы: открытую регистрацию, maintenance mode, название и email поддержки.
public record UpdatePlatformSettingsCommand(
    bool RegistrationOpen,
    bool MaintenanceMode,
    string PlatformName,
    string SupportEmail
) : IRequest<Result<PlatformSettingsDto>>;
