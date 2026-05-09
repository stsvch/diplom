using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

public class Tag : BaseEntity
{
    /// <summary>
    /// Нормализованный ключ тега (lowercase, trim, без спецсимволов). Уникальный.
    /// Используется для поиска и предотвращения дубликатов.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое имя тега в исходной форме (как ввёл пользователь, создавший тег).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Денормализованный счётчик использований — для сортировки в автодополнении.
    /// </summary>
    public int UsageCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<CourseTag> CourseTags { get; set; } = new List<CourseTag>();
}
