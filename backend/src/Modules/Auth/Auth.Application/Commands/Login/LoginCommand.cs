// LoginCommand.cs
using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Login;

// Команда входа хранит email и пароль; успешный обработчик вернёт access token и refresh token.
public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResultDto>>;
