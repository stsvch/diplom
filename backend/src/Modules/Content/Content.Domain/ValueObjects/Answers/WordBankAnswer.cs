// WordBankAnswer.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Answers;

// Value object class: описывает структуру ответа студента для проверки блока.
public class WordBankAnswer : LessonBlockAnswer
{
    public override LessonBlockType Type => LessonBlockType.WordBank;
    public List<WordBankResponse> Responses { get; set; } = new();
}

public class WordBankResponse
{
    public string SentenceId { get; set; } = string.Empty;
    public List<string> Answers { get; set; } = new();
}
