// ImageBlockDataValidator.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

// Валидатор данных блока class: проверяет структуру value object перед сохранением или публикацией.
public class ImageBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Image;

    // Проверяет обязательные поля конкретного типа блока перед сохранением или публикацией.
    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (ImageBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Url))
            return BlockDataValidationResult.Fail("Загрузите изображение или укажите URL.");
        return BlockDataValidationResult.Ok();
    }
}
