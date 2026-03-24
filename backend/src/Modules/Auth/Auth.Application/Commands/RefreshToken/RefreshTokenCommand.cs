using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<Result<LoginResultDto>>;
