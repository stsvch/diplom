// BlockUserCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.BlockUser;

// Команда несёт id блокируемого пользователя и id администратора-инициатора; этого достаточно, чтобы проверить self-action и права на блокировку.
public record BlockUserCommand(string UserId, string ActorUserId) : IRequest<Result<string>>;
