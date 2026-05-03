using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Domain;

namespace Content.Domain.Entities;

public class LessonBlock : BaseEntity, IAuditableEntity
{
    public Guid LessonId { get; set; }
    public int OrderIndex { get; set; }
    public LessonBlockType Type { get; set; }
    public LessonBlockData Data { get; set; } = null!;
    public LessonBlockSettings Settings { get; set; } = new();

    /// <summary>
    /// Статус готовности блока. Draft — допустим без обязательных данных, Ready — прошёл валидацию.
    /// </summary>
    public LessonBlockStatus Status { get; set; } = LessonBlockStatus.Ready;

    /// <summary>
    /// JSON-список ошибок валидации (если блок в Draft или Invalid). Пусто, если блок Ready.
    /// </summary>
    public string? ValidationErrorsJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
