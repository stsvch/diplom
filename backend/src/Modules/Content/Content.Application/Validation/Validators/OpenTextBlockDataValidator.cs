using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class OpenTextBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.OpenText;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (OpenTextBlockData)data;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(d.Instruction))
            errors.Add("Инструкция не может быть пустой.");
        if (d.MinLength.HasValue && d.MaxLength.HasValue && d.MinLength > d.MaxLength)
            errors.Add("Минимальная длина больше максимальной.");
        if (d.MinLength.HasValue && d.MinLength < 0)
            errors.Add("Минимальная длина не может быть отрицательной.");

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
