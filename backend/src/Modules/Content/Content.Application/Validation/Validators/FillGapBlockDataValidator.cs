using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class FillGapBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.FillGap;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (FillGapBlockData)data;
        var errors = new List<string>();

        if (d.Sentences.Count == 0)
            errors.Add("Добавьте хотя бы одно предложение.");

        foreach (var s in d.Sentences)
        {
            if (string.IsNullOrWhiteSpace(s.Template))
                errors.Add($"Предложение {s.Id}: шаблон не может быть пустым.");
            if (s.Gaps.Count == 0)
                errors.Add($"Предложение {s.Id}: должен быть хотя бы один пропуск.");
            foreach (var g in s.Gaps)
            {
                if (g.CorrectAnswers.Count == 0 || g.CorrectAnswers.All(string.IsNullOrWhiteSpace))
                    errors.Add($"Пропуск {g.Id} в предложении {s.Id}: укажите хотя бы один правильный ответ.");
            }
        }

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
