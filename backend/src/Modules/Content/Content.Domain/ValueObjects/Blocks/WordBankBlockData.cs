using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class WordBankBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.WordBank;
    public string? Instruction { get; set; }
    public List<string> Bank { get; set; } = new();
    public List<WordBankSentence> Sentences { get; set; } = new();
    public bool AllowExtraWords { get; set; } = true;
}

public class WordBankSentence
{
    public string Id { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public List<string> CorrectAnswers { get; set; } = new();
}
