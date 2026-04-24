using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.ChangeUserRole;

public record ChangeUserRoleCommand(string UserId, string NewRole, string ActorUserId) : IRequest<Result<string>>;
