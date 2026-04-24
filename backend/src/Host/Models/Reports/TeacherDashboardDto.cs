namespace EduPlatform.Host.Models.Reports;

public class TeacherDashboardDto
{
    public TeacherDashboardSummaryDto Summary { get; set; } = new();
    public TeacherDashboardEarningsDto Earnings { get; set; } = new();
    public List<TeacherDashboardCourseDto> Courses { get; set; } = new();
    public List<TeacherDashboardReviewItemDto> PendingReviews { get; set; } = new();
    public List<TeacherDashboardSessionDto> UpcomingSessions { get; set; } = new();
}

public class TeacherDashboardSummaryDto
{
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int ActiveStudents { get; set; }
    public int PendingReviewsCount { get; set; }
    public decimal AverageStudentProgressPercent { get; set; }
    public decimal AverageGradePercent { get; set; }
    public int UpcomingSessionsCount { get; set; }
}

public class TeacherDashboardEarningsDto
{
    public decimal ReadyForPayoutAmount { get; set; }
    public decimal InPayoutAmount { get; set; }
    public decimal PaidOutAmount { get; set; }
    public string Currency { get; set; } = "usd";
}

public class TeacherDashboardCourseDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public int ActiveStudents { get; set; }
    public int PendingReviewsCount { get; set; }
    public decimal AverageStudentProgressPercent { get; set; }
    public decimal AverageGradePercent { get; set; }
}

public class TeacherDashboardReviewItemDto
{
    public string Kind { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public Guid ReviewId { get; set; }
    public Guid? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class TeacherDashboardSessionDto
{
    public Guid SlotId { get; set; }
    public Guid? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CourseName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsGroupSession { get; set; }
    public int BookingsCount { get; set; }
    public int MaxStudents { get; set; }
}
