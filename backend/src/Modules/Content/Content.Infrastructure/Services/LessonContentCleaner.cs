using Content.Application.Interfaces;
using Content.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Content.Infrastructure.Services;

public class LessonContentCleaner : ILessonContentCleaner
{
    private readonly IContentDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public LessonContentCleaner(IContentDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task DeleteByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        await DeleteInternalAsync(new[] { lessonId }, cancellationToken);
    }

    public async Task DeleteByLessonIdsAsync(IEnumerable<Guid> lessonIds, CancellationToken cancellationToken = default)
    {
        await DeleteInternalAsync(lessonIds, cancellationToken);
    }

    private async Task DeleteInternalAsync(IEnumerable<Guid> lessonIds, CancellationToken cancellationToken)
    {
        var ids = lessonIds.ToList();
        if (ids.Count == 0) return;

        var blocks = await _context.LessonBlocks
            .Where(b => ids.Contains(b.LessonId))
            .ToListAsync(cancellationToken);

        if (blocks.Count == 0) return;

        var blockIds = blocks.Select(b => b.Id).ToList();

        var attachments = await _context.Attachments
            .Where(a => a.EntityType == AttachmentEntityType.LessonBlock && blockIds.Contains(a.EntityId))
            .ToListAsync(cancellationToken);

        foreach (var att in attachments)
        {
            try
            {
                await _fileStorage.DeleteAsync(att.StoragePath, cancellationToken);
            }
            catch
            {
            }
        }

        _context.Attachments.RemoveRange(attachments);
        _context.LessonBlocks.RemoveRange(blocks);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
