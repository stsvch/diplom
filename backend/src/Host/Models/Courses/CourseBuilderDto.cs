using Courses.Domain.Enums;

namespace EduPlatform.Host.Models.Courses;

public enum CourseBuilderReadStatus
{
    Success,
    NotFound,
    Forbidden
}

public sealed record CourseBuilderReadResult(
    CourseBuilderReadStatus Status,
    CourseBuilderDto? Builder = null);

public sealed class CourseBuilderDto
{
    public CourseBuilderCourseDto Course { get; set; } = null!;
    public List<CourseBuilderSectionDto> Sections { get; set; } = new();
    public List<CourseBuilderItemDto> UnsectionedItems { get; set; } = new();
    public CourseBuilderReadinessDto Readiness { get; set; } = new();
}

public sealed class CourseBuilderCourseDto
{
    public Guid Id { get; set; }
    public Guid DisciplineId { get; set; }
    public string DisciplineName { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public CourseLevel Level { get; set; }
    public decimal? Price { get; set; }
    public bool IsFree { get; set; }
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public string? ArchiveReason { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public bool HasGrading { get; set; }
    public bool HasCertificate { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public int StudentsCount { get; set; }
    public int SectionsCount { get; set; }
    public int LessonsCount { get; set; }
    public int TestsCount { get; set; }
    public int AssignmentsCount { get; set; }
    public int LiveSessionsCount { get; set; }
}

public sealed class CourseBuilderSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; }
    public List<CourseBuilderItemDto> Items { get; set; } = new();
}

public sealed class CourseBuilderItemDto
{
    public Guid? CourseItemId { get; set; }
    public Guid SourceId { get; set; }
    public Guid? SectionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public Guid? AttachmentId { get; set; }
    public string? ResourceKind { get; set; }
    public int OrderIndex { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsPublished { get; set; }
    public bool IsRequired { get; set; } = true;
    public decimal? Points { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public int? DurationMinutes { get; set; }
    public int? BlocksCount { get; set; }
    public int? QuestionsCount { get; set; }
    public int? AttemptsCount { get; set; }
    public int? SubmissionsCount { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? MaxStudents { get; set; }
    public int? BookedCount { get; set; }
    public string? MeetingLink { get; set; }
}

public sealed class CourseBuilderReadinessDto
{
    public int TotalItems { get; set; }
    public int ReadyItems { get; set; }
    public decimal ReadyPercent { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public List<CourseBuilderReadinessIssueDto> Issues { get; set; } = new();
}

public sealed class CourseBuilderReadinessIssueDto
{
    public string Severity { get; set; } = "Warning";
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ItemType { get; set; }
    public Guid? SourceId { get; set; }
    public Guid? SectionId { get; set; }
}
