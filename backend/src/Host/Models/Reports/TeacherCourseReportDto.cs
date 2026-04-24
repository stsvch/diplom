namespace EduPlatform.Host.Models.Reports;

public class TeacherCourseReportDto
{
    public TeacherCourseReportSummaryDto Summary { get; set; } = new();
    public List<TeacherCourseGradeBucketDto> GradeDistribution { get; set; } = [];
    public List<TeacherCourseRiskStudentDto> AtRiskStudents { get; set; } = [];
    public List<TeacherCourseDeadlineItemDto> Deadlines { get; set; } = [];
}

public class TeacherCourseReportSummaryDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalLessons { get; set; }
    public decimal AverageProgressPercent { get; set; }
    public decimal CompletionRatePercent { get; set; }
    public decimal AverageGradePercent { get; set; }
    public int PendingReviewsCount { get; set; }
    public int OverdueStudentsCount { get; set; }
    public int OverdueAssignmentsCount { get; set; }
    public int OverdueTestsCount { get; set; }
    public int UpcomingDeadlinesCount { get; set; }
}

public class TeacherCourseGradeBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal SharePercent { get; set; }
}

public class TeacherCourseRiskStudentDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public decimal ProgressPercent { get; set; }
    public decimal? AverageGradePercent { get; set; }
    public int OverdueAssignmentsCount { get; set; }
    public int OverdueTestsCount { get; set; }
    public int PendingReviewCount { get; set; }
}

public class TeacherCourseDeadlineItemDto
{
    public string Kind { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public bool IsOverdue { get; set; }
    public int AffectedStudentsCount { get; set; }
    public int PendingReviewsCount { get; set; }
}
