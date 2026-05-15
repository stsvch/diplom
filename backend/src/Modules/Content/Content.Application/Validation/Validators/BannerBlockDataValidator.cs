// BannerBlockDataValidator.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

// Валидатор данных блока class: проверяет структуру value object перед сохранением или публикацией.
public class BannerBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Banner;

    // Проверяет обязательные поля конкретного типа блока перед сохранением или публикацией.
    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (BannerBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Title))
            return BlockDataValidationResult.Fail("Заголовок баннера не может быть пустым.");
        return BlockDataValidationResult.Ok();
    }
}
