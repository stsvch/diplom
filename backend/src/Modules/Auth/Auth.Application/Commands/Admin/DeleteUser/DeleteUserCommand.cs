using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.DeleteUser;

public record DeleteUserCommand(string UserId) : IRequest<Result<string>>;
