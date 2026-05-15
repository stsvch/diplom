// AudioBlockDataValidator.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

// Валидатор данных блока class: проверяет структуру value object перед сохранением или публикацией.
public class AudioBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Audio;

    // Проверяет обязательные поля конкретного типа блока перед сохранением или публикацией.
    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (AudioBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Url))
            return BlockDataValidationResult.Fail("Укажите URL аудио.");
        return BlockDataValidationResult.Ok();
    }
}
