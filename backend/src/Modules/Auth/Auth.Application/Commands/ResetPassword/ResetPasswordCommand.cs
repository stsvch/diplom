using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<Result<string>>;
