using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class ImageBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Image;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (ImageBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Url))
            return BlockDataValidationResult.Fail("Загрузите изображение или укажите URL.");
        return BlockDataValidationResult.Ok();
    }
}
