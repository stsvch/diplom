// ExecuteCodeCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.CodeExecution;

// Компонент запуска кода record: описывает или выполняет проверку code exercise блоков.
public record ExecuteCodeCommand(
    Guid BlockId,
    Guid UserId,
    string Code
) : IRequest<Result<CodeExecutionResponse>>;
