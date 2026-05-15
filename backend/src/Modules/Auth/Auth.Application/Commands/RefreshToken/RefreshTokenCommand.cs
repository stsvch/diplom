// RefreshTokenCommand.cs
using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.RefreshToken;

// Команда несёт истёкший access token и refresh token из cookie/запроса для безопасной ротации токенов.
public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<Result<LoginResultDto>>;
