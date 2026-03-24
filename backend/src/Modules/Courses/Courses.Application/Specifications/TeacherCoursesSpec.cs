using Ardalis.Specification;
using Courses.Domain.Entities;

namespace Courses.Application.Specifications;

public class TeacherCoursesSpec : Specification<Course>
{
    public TeacherCoursesSpec(string teacherId)
    {
        Query.Where(c => c.TeacherId == teacherId);
        Query.Include(c => c.Discipline);
        Query.Include(c => c.Modules).ThenInclude(m => m.Lessons);
        Query.Include(c => c.Enrollments);
        Query.OrderByDescending(c => c.CreatedAt);
    }
}
