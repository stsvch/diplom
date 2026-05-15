// FileBlockDataValidator.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

// Валидатор данных блока class: проверяет структуру value object перед сохранением или публикацией.
public class FileBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.File;

    // Проверяет обязательные поля конкретного типа блока перед сохранением или публикацией.
    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (FileBlockData)data;
        if (!d.AttachmentId.HasValue || d.AttachmentId == Guid.Empty)
            return BlockDataValidationResult.Fail("Прикрепите файл.");
        return BlockDataValidationResult.Ok();
    }
}
