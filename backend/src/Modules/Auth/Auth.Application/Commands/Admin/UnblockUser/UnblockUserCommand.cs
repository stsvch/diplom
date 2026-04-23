using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.Admin.UnblockUser;

public record UnblockUserCommand(string UserId) : IRequest<Result<string>>;
