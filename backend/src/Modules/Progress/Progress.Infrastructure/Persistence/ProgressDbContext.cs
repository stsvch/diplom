using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Progress.Application.Interfaces;
using Progress.Domain.Entities;

namespace Progress.Infrastructure.Persistence;

public class ProgressDbContext : BaseDbContext, IProgressDbContext
{
    public ProgressDbContext(DbContextOptions<ProgressDbContext> options) : base(options) { }

    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("progress");

        modelBuilder.Entity<LessonProgress>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).IsRequired().HasMaxLength(450);
            e.HasIndex(x => new { x.LessonId, x.StudentId }).IsUnique();
        });
    }
}
