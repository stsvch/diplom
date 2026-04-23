using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

public class TrueFalseAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.TrueFalse;
    public List<TrueFalseResponse> Responses { get; set; } = new();
}

public class TrueFalseResponse
{
    public string StatementId { get; set; } = string.Empty;
    public bool Answer { get; set; }
}
