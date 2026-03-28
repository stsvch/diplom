using EduPlatform.Shared.Domain;

namespace Tests.Domain.Entities;

public class TestResponse : BaseEntity
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }

    public string? SelectedOptionIds { get; set; }
    public string? TextAnswer { get; set; }

    public bool? IsCorrect { get; set; }
    public int? Points { get; set; }
    public string? TeacherComment { get; set; }

    public TestAttempt Attempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
