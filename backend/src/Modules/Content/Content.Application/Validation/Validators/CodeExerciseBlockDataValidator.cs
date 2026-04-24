using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class CodeExerciseBlockDataValidator : IBlockDataValidator
{
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "javascript",
        "python"
    };

    public LessonBlockType SupportedType => LessonBlockType.CodeExercise;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (CodeExerciseBlockData)data;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(d.Instruction))
            errors.Add("Инструкция не может быть пустой.");
        if (string.IsNullOrWhiteSpace(d.Language))
            errors.Add("Выберите язык программирования.");
        else if (!SupportedLanguages.Contains(d.Language))
            errors.Add("Поддерживаются только языки JavaScript и Python.");
        if (d.TestCases.Count == 0)
            errors.Add("Добавьте хотя бы один тест-кейс.");
        if (d.TimeoutMs <= 0 || d.TimeoutMs > 60_000)
            errors.Add("Таймаут должен быть в диапазоне 1..60000 мс.");
        if (d.MemoryLimitMb <= 0 || d.MemoryLimitMb > 1024)
            errors.Add("Лимит памяти должен быть в диапазоне 1..1024 МБ.");

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
