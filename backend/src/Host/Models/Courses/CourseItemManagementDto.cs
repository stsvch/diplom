using Courses.Domain.Enums;

namespace EduPlatform.Host.Models.Courses;

public enum CourseItemMutationStatus
{
    Success,
    NotFound,
    Forbidden,
    ValidationFailed
}

public sealed record CourseItemMutationResult<T>(
    CourseItemMutationStatus Status,
    T? Value = default,
    string? Error = null);

public sealed class CourseItemBackfillDto
{
    public int CreatedItemsCount { get; set; }
    public int LessonsCount { get; set; }
    public int TestsCount { get; set; }
    public int AssignmentsCount { get; set; }
    public int LiveSessionsCount { get; set; }
}

public sealed class CourseItemDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid? SectionId { get; set; }
    public CourseItemType Type { get; set; }
    public Guid SourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public Guid? AttachmentId { get; set; }
    public string? ResourceKind { get; set; }
    public int OrderIndex { get; set; }
    public CourseItemStatus Status { get; set; }
    public bool IsRequired { get; set; }
    public decimal? Points { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? Deadline { get; set; }
}

public sealed record MoveCourseItemRequest(Guid? SectionId, int OrderIndex);

public sealed record ReorderCourseItemsRequest(Guid? SectionId, List<Guid> ItemIds);

public sealed record CreateStandaloneCourseItemRequest(
    CourseItemType Type,
    Guid? SectionId,
    string Title,
    string? Description,
    string? Url,
    Guid? AttachmentId,
    string? ResourceKind,
    bool IsRequired = true,
    decimal? Points = null,
    DateTime? AvailableFrom = null,
    DateTime? Deadline = null);

public sealed record UpdateStandaloneCourseItemRequest(
    string Title,
    string? Description,
    string? Url,
    Guid? AttachmentId,
    string? ResourceKind);

public sealed record UpdateCourseItemMetadataRequest(
    bool IsRequired,
    decimal? Points,
    DateTime? AvailableFrom,
    DateTime? Deadline,
    CourseItemStatus Status);
