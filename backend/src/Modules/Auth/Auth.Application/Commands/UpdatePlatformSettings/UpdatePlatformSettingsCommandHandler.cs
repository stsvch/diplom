using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Commands.UpdatePlatformSettings;

public class UpdatePlatformSettingsCommandHandler : IRequestHandler<UpdatePlatformSettingsCommand, Result<PlatformSettingsDto>>
{
    private readonly IAuthDbContext _context;

    public UpdatePlatformSettingsCommandHandler(IAuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PlatformSettingsDto>> Handle(UpdatePlatformSettingsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PlatformName))
            return Result.Failure<PlatformSettingsDto>("Название платформы обязательно.");
        if (string.IsNullOrWhiteSpace(request.SupportEmail))
            return Result.Failure<PlatformSettingsDto>("Email поддержки обязателен.");

        var settings = await _context.PlatformSettings
            .SingleOrDefaultAsync(s => s.Id == PlatformSetting.SingletonId, cancellationToken);
        if (settings == null)
        {
            settings = new PlatformSetting
            {
                Id = PlatformSetting.SingletonId
            };
            _context.PlatformSettings.Add(settings);
        }

        settings.RegistrationOpen = request.RegistrationOpen;
        settings.MaintenanceMode = request.MaintenanceMode;
        settings.PlatformName = request.PlatformName.Trim();
        settings.SupportEmail = request.SupportEmail.Trim();
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new PlatformSettingsDto
        {
            RegistrationOpen = settings.RegistrationOpen,
            MaintenanceMode = settings.MaintenanceMode,
            PlatformName = settings.PlatformName,
            SupportEmail = settings.SupportEmail
        });
    }
}
