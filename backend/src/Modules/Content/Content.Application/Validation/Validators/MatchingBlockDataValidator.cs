using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class MatchingBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Matching;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (MatchingBlockData)data;
        var errors = new List<string>();

        if (d.LeftItems.Count == 0)
            errors.Add("Добавьте элементы левой колонки.");
        if (d.RightItems.Count == 0)
            errors.Add("Добавьте элементы правой колонки.");
        if (d.CorrectPairs.Count == 0)
            errors.Add("Задайте хотя бы одну пару.");

        var leftIds = d.LeftItems.Select(i => i.Id).ToHashSet();
        var rightIds = d.RightItems.Select(i => i.Id).ToHashSet();

        foreach (var p in d.CorrectPairs)
        {
            if (!leftIds.Contains(p.LeftId))
                errors.Add($"Пара ссылается на несуществующий левый id: {p.LeftId}.");
            if (!rightIds.Contains(p.RightId))
                errors.Add($"Пара ссылается на несуществующий правый id: {p.RightId}.");
        }

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
