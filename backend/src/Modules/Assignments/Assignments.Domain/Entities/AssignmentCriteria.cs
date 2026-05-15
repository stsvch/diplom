// AssignmentCriteria.cs

using EduPlatform.Shared.Domain;

namespace Assignments.Domain.Entities;

/// <summary>
/// Критерий оценивания внутри задания: текст пункта, максимальные баллы и порядок вывода в форме проверки.
/// </summary>
public class AssignmentCriteria : BaseEntity
{
    public Guid AssignmentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int MaxPoints { get; set; }
    public int OrderIndex { get; set; }

    public Assignment Assignment { get; set; } = null!;
}
