using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Queries.GetPlatformSettings;

public record GetPlatformSettingsQuery() : IRequest<Result<PlatformSettingsDto>>;
