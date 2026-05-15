// Tag.cs

using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

// Доменная сущность class: хранит состояние, инварианты и связи бизнес-модели модуля Courses.
public class Tag : BaseEntity
{
    /// <summary>
    /// Нормализованный ключ тега (lowercase, trim, без спецсимволов). Уникальный.
    /// Используется для поиска и предотвращения дубликатов.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// Отображаемое имя тега в исходной форме (как ввёл пользователь, создавший тег).
    public string Name { get; set; } = string.Empty;

    /// Денормализованный счётчик использований — для сортировки в автодополнении.
    public int UsageCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<CourseTag> CourseTags { get; set; } = new List<CourseTag>();
}
