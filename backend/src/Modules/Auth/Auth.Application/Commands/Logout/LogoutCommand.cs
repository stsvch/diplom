// LogoutCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Logout;

// Команда хранит id пользователя, чьи активные refresh-токены нужно отозвать.
public record LogoutCommand(string UserId) : IRequest<Result<string>>;
