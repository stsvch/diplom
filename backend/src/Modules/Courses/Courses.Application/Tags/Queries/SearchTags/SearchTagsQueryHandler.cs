using Courses.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Tags.Queries.SearchTags;

public class SearchTagsQueryHandler : IRequestHandler<SearchTagsQuery, List<TagDto>>
{
    private readonly ICoursesDbContext _context;

    public SearchTagsQueryHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> Handle(SearchTagsQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, 50);
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var q = request.Query.Trim().ToLower();
            query = query.Where(t => t.Slug.Contains(q) || t.Name.ToLower().Contains(q));
        }

        return await query
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .Take(limit)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Slug = t.Slug,
                Name = t.Name,
                UsageCount = t.UsageCount,
            })
            .ToListAsync(cancellationToken);
    }
}
