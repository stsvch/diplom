// AssignmentBlockDataValidator.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

// Валидатор данных блока class: проверяет структуру value object перед сохранением или публикацией.
public class AssignmentBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Assignment;

    // Проверяет обязательные поля конкретного типа блока перед сохранением или публикацией.
    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (AssignmentBlockData)data;
        if (d.AssignmentId == Guid.Empty)
            return BlockDataValidationResult.Fail("Выберите задание.");
        return BlockDataValidationResult.Ok();
    }
}
