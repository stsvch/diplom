namespace Tools.Application.DTOs;

public class DictionaryWordDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? Example { get; set; }
    public List<string> Tags { get; set; } = [];
    public string CreatedById { get; set; } = string.Empty;
    public bool IsKnown { get; set; }
    public int ReviewCount { get; set; }
    public int HardCount { get; set; }
    public int RepeatLaterCount { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public string? LastOutcome { get; set; }
    public DateTime? NextReviewAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
