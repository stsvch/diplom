using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.DeleteUser;

public record DeleteUserCommand(string UserId, string ActorUserId) : IRequest<Result<string>>;
