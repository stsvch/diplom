using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class DropdownBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Dropdown;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (DropdownBlockData)data;
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
                if (g.Options.Count < 2)
                    errors.Add($"Пропуск {g.Id}: нужно хотя бы 2 варианта.");
                if (string.IsNullOrWhiteSpace(g.Correct))
                    errors.Add($"Пропуск {g.Id}: укажите правильный вариант.");
                else if (!g.Options.Contains(g.Correct))
                    errors.Add($"Пропуск {g.Id}: правильный вариант должен быть в списке.");
            }
        }

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
