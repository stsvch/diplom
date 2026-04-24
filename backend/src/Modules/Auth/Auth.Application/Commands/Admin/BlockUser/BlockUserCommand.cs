using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.BlockUser;

public record BlockUserCommand(string UserId, string ActorUserId) : IRequest<Result<string>>;
