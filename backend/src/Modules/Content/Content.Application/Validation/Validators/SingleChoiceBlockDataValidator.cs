using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class SingleChoiceBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.SingleChoice;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (SingleChoiceBlockData)data;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(d.Question))
            errors.Add("Вопрос не может быть пустым.");
        if (d.Options.Count < 2)
            errors.Add("Нужно хотя бы 2 варианта ответа.");
        if (d.Options.Any(o => string.IsNullOrWhiteSpace(o.Text)))
            errors.Add("Все варианты должны содержать текст.");

        var correctCount = d.Options.Count(o => o.IsCorrect);
        if (correctCount == 0)
            errors.Add("Отметьте один правильный вариант.");
        else if (correctCount > 1)
            errors.Add("Для SingleChoice должен быть ровно один правильный вариант.");

        var duplicateIds = d.Options.GroupBy(o => o.Id).Where(g => g.Count() > 1).Any();
        if (duplicateIds)
            errors.Add("У вариантов не должно быть одинаковых id.");

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
