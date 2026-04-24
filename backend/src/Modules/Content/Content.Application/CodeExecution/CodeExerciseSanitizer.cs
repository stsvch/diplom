using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.CodeExecution;

public static class CodeExerciseSanitizer
{
    public static CodeExerciseBlockData SanitizeBlockDataForStudent(CodeExerciseBlockData data)
    {
        return new CodeExerciseBlockData
        {
            Instruction = data.Instruction,
            Language = data.Language,
            StarterCode = data.StarterCode,
            TestCases = data.TestCases
                .Where(t => !t.IsHidden)
                .Select(t => new CodeTestCase
                {
                    Input = t.Input,
                    ExpectedOutput = t.ExpectedOutput,
                    IsHidden = false
                })
                .ToList(),
            TimeoutMs = data.TimeoutMs,
            MemoryLimitMb = data.MemoryLimitMb,
            HiddenTests = data.HiddenTests || data.TestCases.Any(t => t.IsHidden)
        };
    }

    public static CodeExecutionResponse SanitizeExecutionResponseForStudent(CodeExecutionResponse response)
    {
        return response with
        {
            Results = response.Results
                .Select(SanitizeExecutionCaseResultForStudent)
                .ToList()
        };
    }

    public static CodeExerciseAnswer BuildStoredAnswer(
        CodeExerciseBlockData data,
        string code,
        IReadOnlyList<CodeExecutionCaseResult> results)
    {
        return new CodeExerciseAnswer
        {
            Code = code,
            RunOutput = results
                .Select((result, index) => BuildStoredCaseResult(result, IsHidden(data, index)))
                .ToList()
        };
    }

    public static CodeExerciseAnswer SanitizeAnswerForStudent(CodeExerciseAnswer answer, CodeExerciseBlockData? data)
    {
        if (answer.RunOutput is null || answer.RunOutput.Count == 0)
            return answer;

        return new CodeExerciseAnswer
        {
            Code = answer.Code,
            RunOutput = answer.RunOutput
                .Select((result, index) => SanitizeStoredCaseResultForStudent(result, data is not null && IsHidden(data, index)))
                .ToList()
        };
    }

    public static List<CodeTestCaseResult> BuildRunSnapshot(
        CodeExerciseBlockData data,
        IReadOnlyList<CodeExecutionCaseResult> results)
    {
        return results
            .Select((result, index) => new CodeTestCaseResult
            {
                Input = result.Input,
                ExpectedOutput = result.ExpectedOutput,
                ActualOutput = result.ActualOutput,
                Passed = result.Passed,
                IsHidden = result.IsHidden || IsHidden(data, index)
            })
            .ToList();
    }

    public static List<CodeTestCaseResult> CloneRunResults(IEnumerable<CodeTestCaseResult> results)
    {
        return results
            .Select(result => new CodeTestCaseResult
            {
                Input = result.Input,
                ExpectedOutput = result.ExpectedOutput,
                ActualOutput = result.ActualOutput,
                Passed = result.Passed,
                IsHidden = result.IsHidden
            })
            .ToList();
    }

    public static List<CodeTestCaseResult> SanitizeRunResultsForStudent(IEnumerable<CodeTestCaseResult> results)
    {
        return results
            .Select(result => SanitizeStoredCaseResultForStudent(result, false))
            .ToList();
    }

    private static CodeExecutionCaseResult SanitizeExecutionCaseResultForStudent(CodeExecutionCaseResult result)
    {
        if (!result.IsHidden)
            return result;

        return result with
        {
            Input = string.Empty,
            ExpectedOutput = string.Empty,
            ActualOutput = string.Empty,
            Error = string.IsNullOrWhiteSpace(result.Error) ? null : "Скрытый тест не пройден."
        };
    }

    private static CodeTestCaseResult BuildStoredCaseResult(CodeExecutionCaseResult result, bool isHidden)
    {
        return new CodeTestCaseResult
        {
            Input = isHidden ? string.Empty : result.Input,
            ExpectedOutput = isHidden ? string.Empty : result.ExpectedOutput,
            ActualOutput = isHidden ? string.Empty : result.ActualOutput,
            Passed = result.Passed,
            IsHidden = isHidden || result.IsHidden
        };
    }

    private static CodeTestCaseResult SanitizeStoredCaseResultForStudent(CodeTestCaseResult result, bool isHiddenFromBlock)
    {
        var isHidden = result.IsHidden || isHiddenFromBlock;
        if (!isHidden)
            return result;

        return new CodeTestCaseResult
        {
            Input = string.Empty,
            ExpectedOutput = string.Empty,
            ActualOutput = string.Empty,
            Passed = result.Passed,
            IsHidden = true
        };
    }

    private static bool IsHidden(CodeExerciseBlockData data, int index)
    {
        return index >= 0
            && index < data.TestCases.Count
            && data.TestCases[index].IsHidden;
    }
}
