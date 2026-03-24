using Auth.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    UserRole Role
) : IRequest<Result<string>>;
