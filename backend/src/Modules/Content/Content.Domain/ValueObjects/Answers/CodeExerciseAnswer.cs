// CodeExerciseAnswer.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

// Value object class: описывает структуру ответа студента для проверки блока.
public class CodeExerciseAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.CodeExercise;
    public string Code { get; set; } = string.Empty;
    public List<CodeTestCaseResult>? RunOutput { get; set; }
}

public class CodeTestCaseResult
{
    public string Input { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public string ActualOutput { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public bool IsHidden { get; set; }
}
