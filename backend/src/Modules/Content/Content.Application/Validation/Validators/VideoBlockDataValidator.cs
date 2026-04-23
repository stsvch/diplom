using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class VideoBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Video;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (VideoBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Url))
            return BlockDataValidationResult.Fail("Укажите URL видео.");
        return BlockDataValidationResult.Ok();
    }
}
