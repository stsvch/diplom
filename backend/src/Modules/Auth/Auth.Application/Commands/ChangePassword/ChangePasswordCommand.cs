// ChangePasswordCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.ChangePassword;

// Команда несёт id пользователя, текущий пароль и новый пароль; старый пароль нужен для безопасной смены.
public record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Result<string>>;
