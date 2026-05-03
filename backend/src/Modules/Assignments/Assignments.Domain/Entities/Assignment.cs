using Assignments.Domain.Enums;
using EduPlatform.Shared.Domain;

namespace Assignments.Domain.Entities;

public class Assignment : BaseEntity, IAuditableEntity
{
    public Guid? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Текстовая строка с критериями (legacy-формат). Сохранено для обратной совместимости.
    /// Новый структурированный список — в коллекции CriteriaItems (AssignmentCriteria[]).
    /// </summary>
    public string? Criteria { get; set; }

    public DateTime? Deadline { get; set; }
    public int? MaxAttempts { get; set; }
    public int MaxScore { get; set; }

    /// <summary>
    /// Допустимый формат ответа студента: текст / файл / оба варианта.
    /// </summary>
    public AssignmentSubmissionFormat SubmissionFormat { get; set; } = AssignmentSubmissionFormat.Both;

    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();

    /// <summary>
    /// Структурированные критерии оценивания. Каждый со своим максимальным баллом.
    /// </summary>
    public ICollection<AssignmentCriteria> CriteriaItems { get; set; } = new List<AssignmentCriteria>();
}
