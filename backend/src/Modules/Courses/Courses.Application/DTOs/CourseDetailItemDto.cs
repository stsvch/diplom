// CourseDetailItemDto.cs

namespace Courses.Application.DTOs;

// DTO class: передаёт данные наружу из application layer без раскрытия доменных сущностей.
public class CourseDetailItemDto
{
    public Guid Id { get; set; }
    public Guid SourceId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public decimal? Points { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public int? DurationMinutes { get; set; }
    public int? BlocksCount { get; set; }
    public int? QuestionsCount { get; set; }
    public string? Url { get; set; }
    public string? ResourceKind { get; set; }
}
