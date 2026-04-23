using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

public class FillGapAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.FillGap;
    public List<FillGapResponse> Responses { get; set; } = new();
}

public class FillGapResponse
{
    public string SentenceId { get; set; } = string.Empty;
    public List<FillGapValue> Gaps { get; set; } = new();
}

public class FillGapValue
{
    public string GapId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
