using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation;

public class BlockDataValidatorRegistry : IBlockDataValidatorRegistry
{
    private readonly Dictionary<LessonBlockType, IBlockDataValidator> _validators;

    public BlockDataValidatorRegistry(IEnumerable<IBlockDataValidator> validators)
    {
        _validators = validators.ToDictionary(v => v.SupportedType);
    }

    public BlockDataValidationResult Validate(LessonBlockType type, LessonBlockData data)
    {
        if (!_validators.TryGetValue(type, out var validator))
            return BlockDataValidationResult.Ok();

        return validator.Validate(data);
    }
}
