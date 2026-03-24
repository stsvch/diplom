using Ardalis.Specification;
using Courses.Domain.Entities;
using Courses.Domain.Enums;

namespace Courses.Application.Specifications;

public class CourseCatalogCountSpec : Specification<Course>
{
    public CourseCatalogCountSpec(
        Guid? disciplineId,
        bool? isFree,
        CourseLevel? level,
        string? search)
    {
        Query.Where(c => c.IsPublished && !c.IsArchived);

        if (disciplineId.HasValue)
            Query.Where(c => c.DisciplineId == disciplineId.Value);

        if (isFree.HasValue)
            Query.Where(c => c.IsFree == isFree.Value);

        if (level.HasValue)
            Query.Where(c => c.Level == level.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            Query.Where(c => c.Title.ToLower().Contains(searchLower)
                          || c.Description.ToLower().Contains(searchLower));
        }
    }
}
