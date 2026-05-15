// DeleteFileCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Commands.DeleteFile;

// Тип record: ключевой элемент файла DeleteFileCommand.cs.
public record DeleteFileCommand(Guid Id, string UserId) : IRequest<Result<string>>;
