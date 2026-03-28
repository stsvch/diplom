namespace Assignments.Application.DTOs;

public class AssignmentDetailDto : AssignmentDto
{
    public List<SubmissionDto> Submissions { get; set; } = new();
}
