using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class FileBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.File;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (FileBlockData)data;
        if (d.AttachmentId == Guid.Empty)
            return BlockDataValidationResult.Fail("Прикрепите файл.");
        return BlockDataValidationResult.Ok();
    }
}
