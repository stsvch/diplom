using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.CodeExecution;

public record ExecuteCodeCommand(
    Guid BlockId,
    Guid UserId,
    string Code
) : IRequest<Result<CodeExecutionResponse>>;
