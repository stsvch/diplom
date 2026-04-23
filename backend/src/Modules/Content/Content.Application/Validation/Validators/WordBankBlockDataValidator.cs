using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class WordBankBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.WordBank;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (WordBankBlockData)data;
        var errors = new List<string>();

        if (d.Bank.Count == 0)
            errors.Add("Банк слов не может быть пустым.");
        if (d.Sentences.Count == 0)
            errors.Add("Добавьте хотя бы одно предложение.");

        foreach (var s in d.Sentences)
        {
            if (string.IsNullOrWhiteSpace(s.Template))
                errors.Add($"Предложение {s.Id}: шаблон не может быть пустым.");
            if (s.CorrectAnswers.Count == 0)
                errors.Add($"Предложение {s.Id}: укажите правильные ответы.");
            foreach (var word in s.CorrectAnswers)
            {
                if (!d.Bank.Contains(word))
                    errors.Add($"Правильный ответ «{word}» отсутствует в банке слов.");
            }
        }

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
