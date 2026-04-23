using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Progress.Application.Interfaces;
using Progress.Domain.Entities;

namespace Progress.Infrastructure.Services;

public class LessonProgressUpdater : ILessonProgressUpdater
{
    private readonly IProgressDbContext _context;

    public LessonProgressUpdater(IProgressDbContext context)
    {
        _context = context;
    }

    public async Task MarkLessonCompletedAsync(Guid lessonId, Guid userId, CancellationToken cancellationToken = default)
    {
        var studentId = userId.ToString();
        var existing = await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.LessonId == lessonId && p.StudentId == studentId, cancellationToken);

        if (existing is not null)
        {
            if (existing.IsCompleted) return;
            existing.IsCompleted = true;
            existing.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            _context.LessonProgresses.Add(new LessonProgress
            {
                LessonId = lessonId,
                StudentId = studentId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
