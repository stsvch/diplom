using Courses.Domain.Enums;

namespace Courses.Application.DTOs;

public class CourseDetailDto : CourseListDto
{
    public List<CourseModuleDto> Modules { get; set; } = new();
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public CourseOrderType OrderType { get; set; }
    public bool HasGrading { get; set; }
    public bool HasCertificate { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid DisciplineId { get; set; }
}
