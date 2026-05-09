using Content.Application.Interfaces;
using Content.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using Messaging.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EduPlatform.Host.Services;

public class ChatAttachmentCleanupService : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan OrphanAge = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ChatAttachmentCleanupService> _logger;

    public ChatAttachmentCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ChatAttachmentCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat attachment cleanup iteration failed");
            }

            try
            {
                await Task.Delay(RunInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }

    private async Task CleanupOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var contentDb = scope.ServiceProvider.GetRequiredService<IContentDbContext>();
        var mongoDb = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var cleaner = scope.ServiceProvider.GetRequiredService<IAttachmentCleaner>();

        var cutoff = DateTime.UtcNow - OrphanAge;
        var candidates = await contentDb.Attachments
            .Where(a => a.EntityType == AttachmentEntityType.ChatMessage && a.CreatedAt < cutoff)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0) return;

        var messages = mongoDb.GetCollection<MessageDocument>("messages");
        var filter = Builders<MessageDocument>.Filter.AnyIn("Attachments.AttachmentId", candidates);
        var referenced = await messages.Find(filter)
            .Project(m => m.Attachments)
            .ToListAsync(cancellationToken);

        var referencedIds = referenced
            .SelectMany(list => list)
            .Where(a => a.AttachmentId.HasValue)
            .Select(a => a.AttachmentId!.Value)
            .ToHashSet();

        var orphans = candidates.Where(id => !referencedIds.Contains(id)).ToList();
        if (orphans.Count == 0) return;

        await cleaner.DeleteAsync(orphans, cancellationToken);
        _logger.LogInformation("Cleaned up {Count} orphan chat attachments", orphans.Count);
    }
}
