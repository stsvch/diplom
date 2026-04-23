using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Queries.GetPlatformSettings;

public class GetPlatformSettingsQueryHandler : IRequestHandler<GetPlatformSettingsQuery, Result<PlatformSettingsDto>>
{
    private readonly IAuthDbContext _context;

    public GetPlatformSettingsQueryHandler(IAuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PlatformSettingsDto>> Handle(GetPlatformSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.PlatformSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == PlatformSetting.SingletonId, cancellationToken);

        return Result.Success(new PlatformSettingsDto
        {
            RegistrationOpen = settings?.RegistrationOpen ?? true,
            MaintenanceMode = settings?.MaintenanceMode ?? false,
            PlatformName = settings?.PlatformName ?? "EduPlatform",
            SupportEmail = settings?.SupportEmail ?? "support@eduplatform.local"
        });
    }
}
