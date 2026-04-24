using Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Interfaces;

public interface IContentDbContext
{
    DbSet<Attachment> Attachments { get; }
    DbSet<LessonBlock> LessonBlocks { get; }
    DbSet<LessonBlockAttempt> LessonBlockAttempts { get; }
    DbSet<CodeExerciseRun> CodeExerciseRuns { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
