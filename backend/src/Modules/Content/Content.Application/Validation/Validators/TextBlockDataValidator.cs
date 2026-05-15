// TextBlockDataValidator.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

// Валидатор данных блока class: проверяет структуру value object перед сохранением или публикацией.
public class TextBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Text;

    // Проверяет обязательные поля конкретного типа блока перед сохранением или публикацией.
    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (TextBlockData)data;
        if (string.IsNullOrWhiteSpace(d.Html))
            return BlockDataValidationResult.Fail("Текст блока не может быть пустым.");
        return BlockDataValidationResult.Ok();
    }
}
