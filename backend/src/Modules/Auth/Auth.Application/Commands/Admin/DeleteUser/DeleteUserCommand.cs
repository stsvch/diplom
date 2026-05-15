// DeleteUserCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.DeleteUser;

// Команда хранит id удаляемого пользователя и id администратора, чтобы запретить удаление самого себя.
public record DeleteUserCommand(string UserId, string ActorUserId) : IRequest<Result<string>>;
