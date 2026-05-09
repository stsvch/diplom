using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

public class FileBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.File;
    /// <summary>
    /// Null до фактической загрузки файла — File-блок создаётся «черновиком»,
    /// файл прикрепляется отдельным шагом (см. правило о двухшаговом сохранении).
    /// </summary>
    public Guid? AttachmentId { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
}
