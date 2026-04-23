using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class QuizBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Quiz;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (QuizBlockData)data;
        if (d.TestId == Guid.Empty)
            return BlockDataValidationResult.Fail("Выберите тест.");
        return BlockDataValidationResult.Ok();
    }
}
