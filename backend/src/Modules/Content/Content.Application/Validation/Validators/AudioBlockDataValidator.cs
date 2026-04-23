using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class AudioBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Audio;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (AudioBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Url))
            return BlockDataValidationResult.Fail("Укажите URL аудио.");
        return BlockDataValidationResult.Ok();
    }
}
