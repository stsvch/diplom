// StudentEnrolledCoursesSpec.cs

using Ardalis.Specification;
using Courses.Domain.Entities;
using Courses.Domain.Enums;

namespace Courses.Application.Specifications;

// Specification class: инкапсулирует фильтры, сортировку и include-ы для запросов курсов.
public class StudentEnrolledCoursesSpec : Specification<CourseEnrollment>
{
    public StudentEnrolledCoursesSpec(string studentId)
    {
        Query.Where(e =>
            e.StudentId == studentId
            && e.Status == EnrollmentStatus.Active
            && !e.Course.IsArchived);
        Query.Include(e => e.Course).ThenInclude(c => c.Discipline);
        Query.Include(e => e.Course).ThenInclude(c => c.Modules).ThenInclude(m => m.Lessons);
        Query.Include(e => e.Course).ThenInclude(c => c.Enrollments);
        Query.Include(e => e.Course).ThenInclude(c => c.CourseTags).ThenInclude(ct => ct.Tag);
    }
}
