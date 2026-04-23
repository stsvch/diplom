using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;
using Tests.Domain.Enums;

namespace Tests.Infrastructure.Services;

public class TestReadService : ITestReadService
{
    private readonly ITestsDbContext _context;

    public TestReadService(ITestsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TestDeadlineInfo>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Tests
            .Where(t => t.CourseId == courseId)
            .Select(t => new TestDeadlineInfo(t.Id, t.CourseId!.Value, t.Title, t.Deadline))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, DeadlineStatus>> GetStatusesAsync(
        IReadOnlyCollection<Guid> testIds,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (testIds.Count == 0)
            return new Dictionary<Guid, DeadlineStatus>();

        var attempts = await _context.TestAttempts
            .Where(a => testIds.Contains(a.TestId) && a.StudentId == studentId)
            .Select(a => new { a.TestId, a.Status, a.StartedAt, a.AttemptNumber })
            .ToListAsync(cancellationToken);

        return attempts
            .GroupBy(a => a.TestId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var latest = g
                        .OrderByDescending(a => a.StartedAt)
                        .ThenByDescending(a => a.AttemptNumber)
                        .First();
                    return MapStatus(latest.Status);
                });
    }

    private static DeadlineStatus MapStatus(AttemptStatus status) => status switch
    {
        AttemptStatus.Completed => DeadlineStatus.Completed,
        _ => DeadlineStatus.InProgress,
    };
}
