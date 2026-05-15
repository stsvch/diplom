// ICodeExecutor.cs

namespace Content.Application.CodeExecution;

// Тестовый пример для запуска кода: вход, ожидаемый вывод и признак скрытого кейса.
public record CodeExecutionCase(string Input, string ExpectedOutput, bool IsHidden);

// Результат одного тестового запуска кода с фактическим выводом и ошибкой исполнения.
public record CodeExecutionCaseResult(
    string Input,
    string ExpectedOutput,
    string ActualOutput,
    bool Passed,
    bool IsHidden,
    string? Error);

// Запрос к исполнителю кода: язык, исходный код, тесты и ресурсные ограничения.
public record CodeExecutionRequest(
    string Language,
    string Code,
    IReadOnlyList<CodeExecutionCase> TestCases,
    int TimeoutMs,
    int MemoryLimitMb);

// Ответ исполнителя кода: общий статус, результаты тестов и инфраструктурная ошибка.
public record CodeExecutionResponse(
    bool Ok,
    IReadOnlyList<CodeExecutionCaseResult> Results,
    string? GlobalError);

// Компонент запуска кода interface: описывает или выполняет проверку code exercise блоков.
public interface ICodeExecutor
{
    Task<CodeExecutionResponse> ExecuteAsync(CodeExecutionRequest request, CancellationToken cancellationToken = default);
}
