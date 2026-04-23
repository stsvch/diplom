using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class TextBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Text;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (TextBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Html))
            return BlockDataValidationResult.Fail("Текст блока не может быть пустым.");
        return BlockDataValidationResult.Ok();
    }
}
