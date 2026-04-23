namespace Courses.Application.Courses.Commands.PublishCourse;

public record PublishIssue(string Type, string Path, string Code, string Message);

public class PublishValidationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<PublishIssue> Issues { get; set; } = new();

    public bool HasErrors => Issues.Any(i => i.Type == "error");
    public bool HasWarnings => Issues.Any(i => i.Type == "warning");
}
