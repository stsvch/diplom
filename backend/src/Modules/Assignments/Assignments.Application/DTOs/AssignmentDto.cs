namespace Assignments.Application.DTOs;

public class AssignmentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Criteria { get; set; }
    public DateTime? Deadline { get; set; }
    public int? MaxAttempts { get; set; }
    public int MaxScore { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public int SubmissionsCount { get; set; }
}
