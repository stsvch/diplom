// CreateUserCommand.cs
using Auth.Application.Queries.SearchUsers;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.CreateUser;

// Команда содержит email, имя, фамилию, роль и временный пароль для создания аккаунта из админ-панели.
public record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Password
) : IRequest<Result<UserSummaryDto>>;
