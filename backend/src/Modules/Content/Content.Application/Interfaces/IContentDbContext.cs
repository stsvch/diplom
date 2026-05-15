// IContentDbContext.cs

using Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Interfaces;

// Контракт interface: задаёт границу между слоями без привязки application layer к инфраструктуре.
public interface IContentDbContext
{
    DbSet<Attachment> Attachments { get; }
    DbSet<LessonBlock> LessonBlocks { get; }
    DbSet<LessonBlockAttempt> LessonBlockAttempts { get; }
    DbSet<CodeExerciseRun> CodeExerciseRuns { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
