// ChangeUserRoleCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.ChangeUserRole;

// Команда хранит id пользователя, новую роль и id администратора; обработчик по ним меняет роли Identity.
public record ChangeUserRoleCommand(string UserId, string NewRole, string ActorUserId) : IRequest<Result<string>>;
