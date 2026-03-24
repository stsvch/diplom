using Auth.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResultDto>>;
