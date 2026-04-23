using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Application.Validation.Validators;

public class ReorderBlockDataValidator : IBlockDataValidator
{
    public LessonBlockType SupportedType => LessonBlockType.Reorder;

    public BlockDataValidationResult Validate(LessonBlockData data)
    {
        var d = (ReorderBlockData)data;
        var errors = new List<string>();

        if (d.Items.Count < 2)
            errors.Add("Нужно минимум 2 пункта.");
        if (d.Items.Any(i => string.IsNullOrWhiteSpace(i.Text)))
            errors.Add("Все пункты должны содержать текст.");
        if (d.Items.GroupBy(i => i.Id).Any(g => g.Count() > 1))
            errors.Add("У пунктов не должно быть одинаковых id.");
        if (d.CorrectOrder.Count != d.Items.Count)
            errors.Add("Размер правильного порядка не совпадает с количеством пунктов.");
        if (d.CorrectOrder.Distinct().Count() != d.CorrectOrder.Count)
            errors.Add("В правильном порядке есть дубликаты.");
        if (d.CorrectOrder.Any(id => !d.Items.Any(i => i.Id == id)))
            errors.Add("В правильном порядке указан id, которого нет среди пунктов.");

        return errors.Count == 0 ? BlockDataValidationResult.Ok() : BlockDataValidationResult.Fail(errors.ToArray());
    }
}
