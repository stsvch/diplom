// Файл: CourseItemStatusBackfillService.cs
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Tests.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

// Фоновый сервис CourseItemStatusBackfillService выполняет периодическую задачу вне HTTP-запроса.
public class CourseItemStatusBackfillService : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CourseItemStatusBackfillService> _logger;

    public CourseItemStatusBackfillService(
        IServiceScopeFactory scopeFactory,
        ILogger<CourseItemStatusBackfillService> logger)
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

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var coursesDb = scope.ServiceProvider.GetRequiredService<CoursesDbContext>();
            var testsDb = scope.ServiceProvider.GetRequiredService<TestsDbContext>();

            var testItems = await coursesDb.CourseItems
                .Where(i => i.Type == CourseItemType.Test && i.Status == CourseItemStatus.NeedsContent)
                .ToListAsync(stoppingToken);

            if (testItems.Count == 0) return;

            var testIds = testItems.Select(i => i.SourceId).ToList();
            var counts = await testsDb.Questions
                .Where(q => testIds.Contains(q.TestId))
                .GroupBy(q => q.TestId)
                .Select(g => new { TestId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TestId, x => x.Count, stoppingToken);

            var updated = 0;
            foreach (var item in testItems)
            {
                if (counts.TryGetValue(item.SourceId, out var c) && c > 0)
                {
                    item.Status = CourseItemStatus.Ready;
                    updated++;
                }
            }

            if (updated > 0)
            {
                await coursesDb.SaveChangesAsync(stoppingToken);
                _logger.LogInformation(
                    "CourseItemStatusBackfill: promoted {Count} test items from NeedsContent to Ready.",
                    updated);
            }
        }
        catch (Exception ex) when (ex is not TaskCanceledException)
        {
            _logger.LogError(ex, "CourseItemStatusBackfill failed.");
        }
    }
}
