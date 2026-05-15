// UnblockUserCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.UnblockUser;

// Команда передаёт id пользователя, с которого нужно снять блокировку.
public record UnblockUserCommand(string UserId) : IRequest<Result<string>>;
