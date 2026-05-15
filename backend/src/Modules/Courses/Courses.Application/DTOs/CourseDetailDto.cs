// CourseDetailDto.cs

using Courses.Domain.Enums;

namespace Courses.Application.DTOs;

// DTO class: передаёт данные наружу из application layer без раскрытия доменных сущностей.
public class CourseDetailDto : CourseListDto
{
    public List<CourseModuleDto> Modules { get; set; } = new();
    public CourseOrderType OrderType { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid DisciplineId { get; set; }
}
