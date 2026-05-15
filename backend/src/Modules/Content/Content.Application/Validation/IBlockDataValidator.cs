// IBlockDataValidator.cs

using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation;

// Результат проверки данных блока: общий статус и список ошибок для Draft/Ready сценариев.
public record BlockDataValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static BlockDataValidationResult Ok() => new(true, Array.Empty<string>());
    public static BlockDataValidationResult Fail(params string[] errors) => new(false, errors);
}

// Контракт interface: задаёт общий интерфейс проверки данных блоков и единый формат результата.
public interface IBlockDataValidator
{
    LessonBlockType SupportedType { get; }
    BlockDataValidationResult Validate(LessonBlockData data);
}

public interface IBlockDataValidatorRegistry
{
    BlockDataValidationResult Validate(LessonBlockType type, LessonBlockData data);
}
