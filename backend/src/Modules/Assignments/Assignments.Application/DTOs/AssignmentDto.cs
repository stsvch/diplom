using Assignments.Domain.Enums;

namespace Assignments.Application.DTOs;

public class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>Legacy-строка с критериями (сохранена для обратной совместимости).</summary>
    public string? Criteria { get; set; }
    /// <summary>Новый структурированный список критериев оценивания.</summary>
    public List<AssignmentCriteriaDto> CriteriaItems { get; set; } = new();
    public AssignmentSubmissionFormat SubmissionFormat { get; set; } = AssignmentSubmissionFormat.Both;
    public DateTime? Deadline { get; set; }
    public int? MaxAttempts { get; set; }
    public int MaxScore { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public int SubmissionsCount { get; set; }
}
