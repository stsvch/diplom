namespace Assignments.Application.DTOs;

public class AssignmentCriteriaDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int MaxPoints { get; set; }
    public int OrderIndex { get; set; }
}

public record AssignmentCriteriaInput(string Text, int MaxPoints);
