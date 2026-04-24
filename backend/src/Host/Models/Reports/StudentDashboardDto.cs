namespace EduPlatform.Host.Models.Reports;

public class StudentDashboardDto
{
    public StudentDashboardSummaryDto Summary { get; set; } = new();
    public List<StudentDashboardCourseDto> Courses { get; set; } = new();
    public List<StudentDashboardGradeDto> RecentGrades { get; set; } = new();
    public List<StudentDashboardUpcomingItemDto> Upcoming { get; set; } = new();
}

public class StudentDashboardSummaryDto
{
    public int EnrolledCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int CompletedCourses { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public decimal OverallProgressPercent { get; set; }
    public decimal AverageGradePercent { get; set; }
    public int UpcomingEventsCount { get; set; }
}

public class StudentDashboardCourseDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime? Deadline { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public decimal ProgressPercent { get; set; }
    public bool IsCompleted { get; set; }
}

public class StudentDashboardGradeDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Percent { get; set; }
    public DateTime GradedAt { get; set; }
}

public class StudentDashboardUpcomingItemDto
{
    public Guid Id { get; set; }
    public Guid? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? EventTime { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string? Status { get; set; }
}
