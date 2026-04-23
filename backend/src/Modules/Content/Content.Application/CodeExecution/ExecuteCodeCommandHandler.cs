using Content.Application.Interfaces;
using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.CodeExecution;

public class ExecuteCodeCommandHandler : IRequestHandler<ExecuteCodeCommand, Result<CodeExecutionResponse>>
{
    private readonly IContentDbContext _context;
    private readonly ICodeExecutor _executor;

    public ExecuteCodeCommandHandler(IContentDbContext context, ICodeExecutor executor)
    {
        _context = context;
        _executor = executor;
    }

    public async Task<Result<CodeExecutionResponse>> Handle(ExecuteCodeCommand request, CancellationToken cancellationToken)
    {
        var block = await _context.LessonBlocks.FindAsync([request.BlockId], cancellationToken);
        if (block is null)
            return Result.Failure<CodeExecutionResponse>("Блок не найден.");

        if (block.Type != LessonBlockType.CodeExercise || block.Data is not CodeExerciseBlockData data)
            return Result.Failure<CodeExecutionResponse>("Этот блок не является упражнением по коду.");

        var cases = data.TestCases.Select(t => new CodeExecutionCase(t.Input, t.ExpectedOutput, t.IsHidden)).ToList();
        var req = new CodeExecutionRequest(data.Language, request.Code, cases, data.TimeoutMs);

        var response = await _executor.ExecuteAsync(req, cancellationToken);
        return Result.Success(response);
    }
}
