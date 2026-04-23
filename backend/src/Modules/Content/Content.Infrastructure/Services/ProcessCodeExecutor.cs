using System.Diagnostics;
using Content.Application.CodeExecution;

namespace Content.Infrastructure.Services;

/// <summary>
/// Примитивная реализация запуска кода через дочерний процесс.
/// MVP. Не изолирован — для production нужен отдельный sandbox (Docker, gVisor, Firecracker).
/// </summary>
public class ProcessCodeExecutor : ICodeExecutor
{
    public async Task<CodeExecutionResponse> ExecuteAsync(CodeExecutionRequest request, CancellationToken cancellationToken = default)
    {
        var lang = request.Language.ToLowerInvariant();
        var (exe, ext, stdinMode) = lang switch
        {
            "python" => ("python", "py", true),
            "javascript" => ("node", "js", true),
            _ => (null, null, true),
        };

        if (exe is null)
        {
            return new CodeExecutionResponse(false, Array.Empty<CodeExecutionCaseResult>(),
                $"Язык {request.Language} пока не поддерживается исполнителем. Доступны: python, javascript.");
        }

        var tmpDir = Path.Combine(Path.GetTempPath(), "eduplatform-code-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        var codeFile = Path.Combine(tmpDir, "program." + ext);
        await File.WriteAllTextAsync(codeFile, request.Code, cancellationToken);

        var results = new List<CodeExecutionCaseResult>();
        try
        {
            foreach (var tc in request.TestCases)
            {
                var result = await RunOnceAsync(exe!, codeFile, tc, request.TimeoutMs, cancellationToken);
                results.Add(result);
            }
        }
        finally
        {
            try { Directory.Delete(tmpDir, recursive: true); } catch { }
        }

        return new CodeExecutionResponse(true, results, null);
    }

    private static async Task<CodeExecutionCaseResult> RunOnceAsync(
        string exe, string codeFile, CodeExecutionCase tc, int timeoutMs, CancellationToken ct)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = $"\"{codeFile}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            if (proc is null)
                return new CodeExecutionCaseResult(tc.Input, tc.ExpectedOutput, "", false, tc.IsHidden, $"Не удалось запустить {exe}");

            if (!string.IsNullOrEmpty(tc.Input))
            {
                await proc.StandardInput.WriteLineAsync(tc.Input);
                proc.StandardInput.Close();
            }

            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            try
            {
                await proc.WaitForExitAsync(linked.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                try { proc.Kill(entireProcessTree: true); } catch { }
                return new CodeExecutionCaseResult(tc.Input, tc.ExpectedOutput, "", false, tc.IsHidden, "Таймаут");
            }

            var stdout = (await proc.StandardOutput.ReadToEndAsync()).Trim();
            var stderr = (await proc.StandardError.ReadToEndAsync()).Trim();

            if (proc.ExitCode != 0 && !string.IsNullOrEmpty(stderr))
                return new CodeExecutionCaseResult(tc.Input, tc.ExpectedOutput, stdout, false, tc.IsHidden, stderr);

            var passed = string.Equals(stdout.Trim(), tc.ExpectedOutput.Trim(), StringComparison.Ordinal);
            return new CodeExecutionCaseResult(tc.Input, tc.ExpectedOutput, stdout, passed, tc.IsHidden, null);
        }
        catch (Exception e)
        {
            return new CodeExecutionCaseResult(tc.Input, tc.ExpectedOutput, "", false, tc.IsHidden, e.Message);
        }
    }
}
