// ResetPasswordCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.ResetPassword;

// Команда хранит email, reset token и новый пароль, полученные из формы восстановления.
public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<Result<string>>;
