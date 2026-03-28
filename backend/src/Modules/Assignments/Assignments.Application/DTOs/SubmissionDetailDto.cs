namespace Assignments.Application.DTOs;

public class SubmissionDetailDto : SubmissionDto
{
    public List<string> AttachmentFileNames { get; set; } = new();
}
