namespace Courses.Application.Courses.Queries.GetCourseStats;

public class CourseStatsDto
{
    public int Total { get; set; }
    public int Published { get; set; }
    public int Drafts { get; set; }
    public int Archived { get; set; }
    public int TotalEnrollments { get; set; }
    public int Disciplines { get; set; }
}
