using Content.Application.Interfaces;
using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Content.Infrastructure.Services;

public class AttachmentCleaner : IAttachmentCleaner
{
    private readonly IContentDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<AttachmentCleaner> _logger;

    public AttachmentCleaner(
        IContentDbContext context,
        IFileStorageService fileStorage,
        ILogger<AttachmentCleaner> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task DeleteAsync(IEnumerable<Guid> attachmentIds, CancellationToken cancellationToken = default)
    {
        var ids = attachmentIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var attachments = await _context.Attachments
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);

        if (attachments.Count == 0) return;

        foreach (var att in attachments)
        {
            try
            {
                await _fileStorage.DeleteAsync(att.StoragePath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {StoragePath} from storage", att.StoragePath);
            }
        }

        _context.Attachments.RemoveRange(attachments);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
