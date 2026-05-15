// CourseStatsDto.cs

namespace Courses.Application.Courses.Queries.GetCourseStats;

// Тип class: ключевой элемент файла CourseStatsDto.cs.
public class CourseStatsDto
{
    public int Total { get; set; }
    public int Published { get; set; }
    public int Drafts { get; set; }
    public int Archived { get; set; }
    public int TotalEnrollments { get; set; }
    public int Disciplines { get; set; }
}
