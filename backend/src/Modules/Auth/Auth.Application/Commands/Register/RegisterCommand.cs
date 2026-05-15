// RegisterCommand.cs
using Auth.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Register;

// Команда регистрации содержит email, пароль, имя, фамилию и роль будущего Student/Teacher аккаунта.
public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    UserRole Role
) : IRequest<Result<string>>;
