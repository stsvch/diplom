using EduPlatform.Shared.Domain;

namespace Tests.Domain.Entities;

public class Test : BaseEntity, IAuditableEntity
{
    public Guid? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatedById { get; set; } = string.Empty;

    public int? TimeLimitMinutes { get; set; }
    public int? MaxAttempts { get; set; }
    public DateTime? Deadline { get; set; }

    public bool ShuffleQuestions { get; set; }
    public bool ShuffleAnswers { get; set; }
    public bool ShowCorrectAnswers { get; set; }

    public int MaxScore { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
