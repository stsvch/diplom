using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class MultipleChoiceBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.MultipleChoice;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (MultipleChoiceBlockData)data;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(d.Question))
            errors.Add("Вопрос не может быть пустым.");
        if (d.Options.Count < 2)
            errors.Add("Нужно хотя бы 2 варианта ответа.");
        if (d.Options.Any(o => string.IsNullOrWhiteSpace(o.Text)))
            errors.Add("Все варианты должны содержать текст.");
        if (!d.Options.Any(o => o.IsCorrect))
            errors.Add("Отметьте хотя бы один правильный вариант.");
        if (d.Options.GroupBy(o => o.Id).Any(g => g.Count() > 1))
            errors.Add("У вариантов не должно быть одинаковых id.");

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
