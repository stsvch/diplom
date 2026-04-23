using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class TrueFalseBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.TrueFalse;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (TrueFalseBlockData)data;
        var errors = new List<string>();

        if (d.Statements.Count == 0)
            errors.Add("Добавьте хотя бы одно утверждение.");
        if (d.Statements.Any(s => string.IsNullOrWhiteSpace(s.Text)))
            errors.Add("Все утверждения должны содержать текст.");
        if (d.Statements.GroupBy(s => s.Id).Any(g => g.Count() > 1))
            errors.Add("У утверждений не должно быть одинаковых id.");

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
