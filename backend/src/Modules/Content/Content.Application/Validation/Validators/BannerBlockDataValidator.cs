using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class BannerBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Banner;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (BannerBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Title))
            return BlockDataValidationResult.Fail("Заголовок баннера не может быть пустым.");
        return BlockDataValidationResult.Ok();
    }
}
