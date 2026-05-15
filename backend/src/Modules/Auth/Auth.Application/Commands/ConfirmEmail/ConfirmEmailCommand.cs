// ConfirmEmailCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.ConfirmEmail;

// Команда содержит userId и token из email-ссылки подтверждения.
public record ConfirmEmailCommand(
    string UserId,
    string Token
) : IRequest<Result<string>>;
