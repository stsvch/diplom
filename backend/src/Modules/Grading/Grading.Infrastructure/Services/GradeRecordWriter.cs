using EduPlatform.Shared.Application.Contracts;
using Grading.Application.Interfaces;
using Grading.Domain.Entities;
using Grading.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Grading.Infrastructure.Services;

public class GradeRecordWriter : IGradeRecordWriter
{
    private readonly IGradingDbContext _context;

    public GradeRecordWriter(IGradingDbContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(GradeRecordUpsert request, CancellationToken cancellationToken = default)
    {
        var existing = await FindExistingAsync(request, cancellationToken);
        if (existing is null)
        {
            existing = new Grade();
            _context.Grades.Add(existing);
        }

        existing.StudentId = request.StudentId;
        existing.CourseId = request.CourseId;
        existing.SourceType = ParseSourceType(request.SourceType);
        existing.TestAttemptId = request.TestAttemptId;
        existing.AssignmentSubmissionId = request.AssignmentSubmissionId;
        existing.Title = request.Title;
        existing.Score = request.Score;
        existing.MaxScore = request.MaxScore;
        existing.Comment = request.Comment;
        existing.GradedAt = request.GradedAt;
        existing.GradedById = request.GradedById;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByTestAttemptAsync(Guid testAttemptId, CancellationToken cancellationToken = default)
    {
        var grades = await _context.Grades
            .Where(g => g.TestAttemptId == testAttemptId)
            .ToListAsync(cancellationToken);

        if (grades.Count == 0)
            return;

        _context.Grades.RemoveRange(grades);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByAssignmentSubmissionAsync(Guid assignmentSubmissionId, CancellationToken cancellationToken = default)
    {
        var grades = await _context.Grades
            .Where(g => g.AssignmentSubmissionId == assignmentSubmissionId)
            .ToListAsync(cancellationToken);

        if (grades.Count == 0)
            return;

        _context.Grades.RemoveRange(grades);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Grade?> FindExistingAsync(GradeRecordUpsert request, CancellationToken cancellationToken)
    {
        if (request.TestAttemptId.HasValue)
        {
            return await _context.Grades
                .FirstOrDefaultAsync(g => g.TestAttemptId == request.TestAttemptId.Value, cancellationToken);
        }

        if (request.AssignmentSubmissionId.HasValue)
        {
            return await _context.Grades
                .FirstOrDefaultAsync(g => g.AssignmentSubmissionId == request.AssignmentSubmissionId.Value, cancellationToken);
        }

        return null;
    }

    private static GradeSourceType ParseSourceType(string sourceType) =>
        Enum.TryParse<GradeSourceType>(sourceType, true, out var parsed)
            ? parsed
            : throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, "Unsupported grade source type.");
}
