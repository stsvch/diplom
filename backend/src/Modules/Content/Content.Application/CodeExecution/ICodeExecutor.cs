namespace Content.Application.CodeExecution;

public record CodeExecutionCase(string Input, string ExpectedOutput, bool IsHidden);

public record CodeExecutionCaseResult(
    string Input,
    string ExpectedOutput,
    string ActualOutput,
    bool Passed,
    bool IsHidden,
    string? Error);

public record CodeExecutionRequest(
    string Language,
    string Code,
    IReadOnlyList<CodeExecutionCase> TestCases,
    int TimeoutMs);

public record CodeExecutionResponse(
    bool Ok,
    IReadOnlyList<CodeExecutionCaseResult> Results,
    string? GlobalError);

public interface ICodeExecutor
{
    Task<CodeExecutionResponse> ExecuteAsync(CodeExecutionRequest request, CancellationToken cancellationToken = default);
}
