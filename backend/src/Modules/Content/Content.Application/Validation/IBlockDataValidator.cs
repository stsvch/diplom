using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation;

public record BlockDataValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static BlockDataValidationResult Ok() => new(true, Array.Empty<string>());
    public static BlockDataValidationResult Fail(params string[] errors) => new(false, errors);
}

public interface IBlockDataValidator
{
    LessonBlockType SupportedType { get; }
    BlockDataValidationResult Validate(LessonBlockData data);
}

public interface IBlockDataValidatorRegistry
{
    BlockDataValidationResult Validate(LessonBlockType type, LessonBlockData data);
}
