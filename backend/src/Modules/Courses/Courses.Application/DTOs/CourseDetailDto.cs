using Courses.Domain.Enums;

namespace Courses.Application.DTOs;

public class CourseDetailDto : CourseListDto
{
    public List<CourseModuleDto> Modules { get; set; } = new();
    public CourseOrderType OrderType { get; set; }
    public bool HasGrading { get; set; }
    public bool HasCertificate { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid DisciplineId { get; set; }
}
