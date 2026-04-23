using Auth.Application.Queries.SearchUsers;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.CreateUser;

public record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Password
) : IRequest<Result<UserSummaryDto>>;
