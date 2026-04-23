using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class CodeExerciseBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.CodeExercise;
    public string Instruction { get; set; } = string.Empty;
    public string Language { get; set; } = "csharp";
    public string? StarterCode { get; set; }
    public List<CodeTestCase> TestCases { get; set; } = new();
    public int TimeoutMs { get; set; } = 5000;
    public int MemoryLimitMb { get; set; } = 128;
    public bool HiddenTests { get; set; }
}

public class CodeTestCase
{
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
}
