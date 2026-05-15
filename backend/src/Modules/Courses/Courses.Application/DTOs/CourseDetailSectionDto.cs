// CourseDetailSectionDto.cs

namespace Courses.Application.DTOs;

// DTO class: передаёт данные наружу из application layer без раскрытия доменных сущностей.
public class CourseDetailSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; }
    public List<CourseDetailItemDto> Items { get; set; } = new();
}
