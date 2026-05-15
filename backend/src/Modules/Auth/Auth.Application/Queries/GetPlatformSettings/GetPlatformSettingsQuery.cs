// GetPlatformSettingsQuery.cs
using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.GetPlatformSettings;

// Запрос текущих платформенных флагов; используется публичными страницами и админскими настройками.
public record GetPlatformSettingsQuery() : IRequest<Result<PlatformSettingsDto>>;
