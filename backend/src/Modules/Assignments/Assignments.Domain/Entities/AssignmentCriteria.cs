using EduPlatform.Shared.Domain;

namespace Assignments.Domain.Entities;

/// <summary>
/// Критерий оценивания задания. У задания может быть несколько критериев,
/// каждый со своим максимальным баллом. Сумма MaxPoints критериев = Assignment.MaxScore.
/// </summary>
public class AssignmentCriteria : BaseEntity
{
    public Guid AssignmentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int MaxPoints { get; set; }
    public int OrderIndex { get; set; }

    public Assignment Assignment { get; set; } = null!;
}
