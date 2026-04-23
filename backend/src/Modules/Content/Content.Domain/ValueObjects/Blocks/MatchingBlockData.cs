using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class MatchingBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Matching;
    public string? Instruction { get; set; }
    public List<MatchingItem> LeftItems { get; set; } = new();
    public List<MatchingItem> RightItems { get; set; } = new();
    public List<MatchingPair> CorrectPairs { get; set; } = new();
}

public class MatchingItem
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class MatchingPair
{
    public string LeftId { get; set; } = string.Empty;
    public string RightId { get; set; } = string.Empty;
}
