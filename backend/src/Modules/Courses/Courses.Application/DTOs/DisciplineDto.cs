// DisciplineDto.cs

namespace Courses.Application.DTOs;

// DTO class: передаёт данные наружу из application layer без раскрытия доменных сущностей.
public class DisciplineDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CourseCount { get; set; }
}
