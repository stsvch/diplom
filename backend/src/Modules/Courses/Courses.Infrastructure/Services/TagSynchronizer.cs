using System.Globalization;
using System.Text;
using Courses.Application.Interfaces;
using Courses.Application.Tags;
using Courses.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Courses.Infrastructure.Services;

public class TagSynchronizer : ITagSynchronizer
{
    private readonly ICoursesDbContext _context;

    public TagSynchronizer(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task SyncAsync(Guid courseId, IEnumerable<string>? rawTagNames, CancellationToken cancellationToken)
    {
        var normalized = NormalizeAndDedupe(rawTagNames);

        var existingLinks = await _context.CourseTags
            .Where(ct => ct.CourseId == courseId)
            .ToListAsync(cancellationToken);

        if (normalized.Count == 0)
        {
            if (existingLinks.Count > 0)
            {
                _context.CourseTags.RemoveRange(existingLinks);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        var slugs = normalized.Select(n => n.Slug).ToList();
        var existingTags = await _context.Tags
            .Where(t => slugs.Contains(t.Slug))
            .ToListAsync(cancellationToken);

        var slugToTag = existingTags.ToDictionary(t => t.Slug, t => t);

        foreach (var (slug, displayName) in normalized)
        {
            if (slugToTag.ContainsKey(slug)) continue;
            var tag = new Tag
            {
                Slug = slug,
                Name = displayName,
                CreatedAt = DateTime.UtcNow,
            };
            _context.Tags.Add(tag);
            slugToTag[slug] = tag;
        }

        var desiredTagIds = normalized.Select(n => slugToTag[n.Slug].Id).ToHashSet();
        var existingTagIds = existingLinks.Select(l => l.TagId).ToHashSet();

        var toRemove = existingLinks.Where(l => !desiredTagIds.Contains(l.TagId)).ToList();
        if (toRemove.Count > 0)
            _context.CourseTags.RemoveRange(toRemove);

        foreach (var tagId in desiredTagIds.Where(id => !existingTagIds.Contains(id)))
        {
            _context.CourseTags.Add(new CourseTag { CourseId = courseId, TagId = tagId });
        }

        // Пересчёт UsageCount только для затронутых тегов
        await _context.SaveChangesAsync(cancellationToken);
        await RecalculateUsageAsync(desiredTagIds.Concat(toRemove.Select(l => l.TagId)).Distinct(), cancellationToken);
    }

    private async Task RecalculateUsageAsync(IEnumerable<Guid> tagIds, CancellationToken ct)
    {
        var ids = tagIds.ToList();
        if (ids.Count == 0) return;

        var counts = await _context.CourseTags
            .Where(ct => ids.Contains(ct.TagId))
            .GroupBy(ct => ct.TagId)
            .Select(g => new { TagId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countByTag = counts.ToDictionary(c => c.TagId, c => c.Count);

        var tags = await _context.Tags.Where(t => ids.Contains(t.Id)).ToListAsync(ct);
        foreach (var tag in tags)
        {
            tag.UsageCount = countByTag.TryGetValue(tag.Id, out var count) ? count : 0;
        }
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Парсит входные имена в список (slug, displayName), удаляет пустые и дубли по slug.
    /// </summary>
    private static List<(string Slug, string DisplayName)> NormalizeAndDedupe(IEnumerable<string>? raw)
    {
        if (raw is null) return new();

        var result = new List<(string, string)>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in raw)
        {
            if (string.IsNullOrWhiteSpace(item)) continue;
            var displayName = item.Trim();
            if (displayName.Length > 80) displayName = displayName[..80];

            var slug = MakeSlug(displayName);
            if (string.IsNullOrEmpty(slug)) continue;
            if (!seen.Add(slug)) continue;

            result.Add((slug, displayName));
        }

        return result;
    }

    /// <summary>
    /// Lowercase, trim, схлопывание пробелов и спецсимволов в дефис. Поддерживает кириллицу.
    /// </summary>
    private static string MakeSlug(string input)
    {
        var lower = input.Trim().ToLower(CultureInfo.InvariantCulture);
        var sb = new StringBuilder(lower.Length);
        var prevDash = false;
        foreach (var ch in lower)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                prevDash = false;
            }
            else if (!prevDash && sb.Length > 0)
            {
                sb.Append('-');
                prevDash = true;
            }
        }
        var slug = sb.ToString().TrimEnd('-');
        return slug.Length > 80 ? slug[..80] : slug;
    }
}
