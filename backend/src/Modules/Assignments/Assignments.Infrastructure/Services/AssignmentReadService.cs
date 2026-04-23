using Assignments.Application.Interfaces;
using Assignments.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Infrastructure.Services;

public class AssignmentReadService : IAssignmentReadService
{
    private readonly IAssignmentsDbContext _context;

    public AssignmentReadService(IAssignmentsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AssignmentDeadlineInfo>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Assignments
            .Where(a => a.CourseId == courseId)
            .Select(a => new AssignmentDeadlineInfo(a.Id, a.CourseId!.Value, a.Title, a.Deadline))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, DeadlineStatus>> GetStatusesAsync(
        IReadOnlyCollection<Guid> assignmentIds,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (assignmentIds.Count == 0)
            return new Dictionary<Guid, DeadlineStatus>();

        var submissions = await _context.AssignmentSubmissions
            .Where(s => assignmentIds.Contains(s.AssignmentId) && s.StudentId == studentId)
            .Select(s => new { s.AssignmentId, s.Status, s.SubmittedAt, s.AttemptNumber })
            .ToListAsync(cancellationToken);

        return submissions
            .GroupBy(s => s.AssignmentId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var latest = g
                        .OrderByDescending(s => s.SubmittedAt)
                        .ThenByDescending(s => s.AttemptNumber)
                        .First();
                    return MapStatus(latest.Status);
                });
    }

    private static DeadlineStatus MapStatus(SubmissionStatus status) => status switch
    {
        SubmissionStatus.Graded => DeadlineStatus.Completed,
        SubmissionStatus.ReturnedForRevision => DeadlineStatus.Pending,
        _ => DeadlineStatus.InProgress,
    };
}
