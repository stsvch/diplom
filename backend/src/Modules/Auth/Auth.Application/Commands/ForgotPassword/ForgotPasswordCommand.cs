using EduPlatform.Shared.Domain;
using MediatR;

namespace Auth.Application.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;
