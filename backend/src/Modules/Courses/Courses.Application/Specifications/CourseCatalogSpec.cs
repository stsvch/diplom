using Ardalis.Specification;
using Courses.Domain.Entities;
using Courses.Domain.Enums;

namespace Courses.Application.Specifications;

public class CourseCatalogSpec : Specification<Course>
{
    public CourseCatalogSpec(
        Guid? disciplineId,
        bool? isFree,
        CourseLevel? level,
        string? search,
        string? sortBy,
        int page,
        int pageSize)
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

        Query.Include(c => c.Discipline);
        Query.Include(c => c.Modules).ThenInclude(m => m.Lessons);
        Query.Include(c => c.Enrollments);

        switch (sortBy?.ToLower())
        {
            case "price":
                Query.OrderBy(c => c.Price ?? 0);
                break;
            case "price_desc":
                Query.OrderByDescending(c => c.Price ?? 0);
                break;
            case "newest":
                Query.OrderByDescending(c => c.CreatedAt);
                break;
            case "popular":
                Query.OrderByDescending(c => c.Enrollments.Count);
                break;
            default:
                Query.OrderByDescending(c => c.CreatedAt);
                break;
        }

        Query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}
