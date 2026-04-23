using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.ChangePassword;

public record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Result<string>>;
