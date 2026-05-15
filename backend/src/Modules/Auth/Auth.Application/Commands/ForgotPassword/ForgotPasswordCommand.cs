// ForgotPasswordCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.ForgotPassword;

// Команда содержит email, для которого нужно отправить ссылку восстановления пароля.
public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;
