namespace Tests.Application.DTOs;

public class TestDto
{
    public Guid Id { get; set; }
    public Guid? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? MaxAttempts { get; set; }
    public DateTime? Deadline { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleAnswers { get; set; }
    public bool ShowCorrectAnswers { get; set; }
    public int MaxScore { get; set; }
    public int QuestionsCount { get; set; }
    public string CreatedById { get; set; } = string.Empty;
}
