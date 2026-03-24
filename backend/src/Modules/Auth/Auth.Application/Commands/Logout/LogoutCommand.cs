using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Logout;

public record LogoutCommand(string UserId) : IRequest<Result<string>>;
