using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Commands.DeleteFile;

public record DeleteFileCommand(Guid Id, string UserId) : IRequest<Result<string>>;
