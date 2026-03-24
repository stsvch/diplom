using Ardalis.Specification;
using Courses.Domain.Entities;

namespace Courses.Application.Specifications;

public class CourseByIdSpec : Specification<Course>
{
    public CourseByIdSpec(Guid id)
    {
        Query.Where(c => c.Id == id);
        Query.Include(c => c.Discipline);
        Query.Include(c => c.Modules).ThenInclude(m => m.Lessons).ThenInclude(l => l.Blocks);
        Query.Include(c => c.Enrollments);
    }
}
