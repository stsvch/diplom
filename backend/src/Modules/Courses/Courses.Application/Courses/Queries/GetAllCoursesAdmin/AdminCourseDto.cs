namespace Courses.Application.Courses.Queries.GetAllCoursesAdmin;

public class AdminCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public Guid DisciplineId { get; set; }
    public string DisciplineName { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public string? ArchiveReason { get; set; }
    public int StudentsCount { get; set; }
    public int ModulesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
