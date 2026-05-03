using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Progress.Application.Interfaces;
using Progress.Domain.Entities;

namespace Progress.Infrastructure.Persistence;

public class ProgressDbContext : BaseDbContext, IProgressDbContext
{
    public ProgressDbContext(DbContextOptions<ProgressDbContext> options) : base(options) { }

    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<CourseItemProgress> CourseItemProgresses => Set<CourseItemProgress>();

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

        modelBuilder.Entity<CourseItemProgress>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).IsRequired().HasMaxLength(450);
            e.Property(x => x.ItemType).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.CourseId);
            e.HasIndex(x => x.StudentId);
            e.HasIndex(x => new { x.CourseItemId, x.StudentId }).IsUnique();
        });
    }
}
