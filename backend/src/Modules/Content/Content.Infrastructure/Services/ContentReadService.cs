using Content.Application.Interfaces;
using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Content.Infrastructure.Services;

public class ContentReadService : IContentReadService
{
    private readonly IContentDbContext _context;

    public ContentReadService(IContentDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetBlocksCountAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _context.LessonBlocks.CountAsync(b => b.LessonId == lessonId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetBlocksCountByLessonIdsAsync(
        IEnumerable<Guid> lessonIds,
        CancellationToken cancellationToken = default)
    {
        var ids = lessonIds.ToList();
        var counts = await _context.LessonBlocks
            .Where(b => ids.Contains(b.LessonId))
            .GroupBy(b => b.LessonId)
            .Select(g => new { LessonId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.LessonId, x => x.Count);
    }
}
