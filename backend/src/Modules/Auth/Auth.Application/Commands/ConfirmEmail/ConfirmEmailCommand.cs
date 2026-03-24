using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    string UserId,
    string Token
) : IRequest<Result<string>>;
