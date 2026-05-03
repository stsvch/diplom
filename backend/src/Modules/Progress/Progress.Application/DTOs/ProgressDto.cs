namespace Progress.Application.DTOs;

public class LessonProgressDto
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CourseProgressDto
{
    public Guid CourseId { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public decimal ProgressPercent { get; set; }
}

public class CourseItemProgressDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid CourseItemId { get; set; }
    public Guid SourceId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class MyProgressDto
{
    public List<CourseProgressDto> Courses { get; set; } = new();
}
