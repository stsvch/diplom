using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.CodeExecution;

public record ExecuteCodeCommand(
    Guid BlockId,
    string Code
) : IRequest<Result<CodeExecutionResponse>>;
