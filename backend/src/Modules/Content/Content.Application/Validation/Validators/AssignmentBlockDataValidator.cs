using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class AssignmentBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Assignment;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (AssignmentBlockData)data;
        if (d.AssignmentId == Guid.Empty)
            return BlockDataValidationResult.Fail("Выберите задание.");
        return BlockDataValidationResult.Ok();
    }
}
