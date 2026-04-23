using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

public class MatchingAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.Matching;
    public List<MatchingAnswerPair> Pairs { get; set; } = new();
}

public class MatchingAnswerPair
{
    public string LeftId { get; set; } = string.Empty;
    public string RightId { get; set; } = string.Empty;
}
