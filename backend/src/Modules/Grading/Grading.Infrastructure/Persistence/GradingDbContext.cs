using EduPlatform.Shared.Infrastructure.Persistence;
using Grading.Application.Interfaces;
using Grading.Domain.Entities;
using Grading.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Grading.Infrastructure.Persistence;

public class GradingDbContext : BaseDbContext, IGradingDbContext
{
    public GradingDbContext(DbContextOptions<GradingDbContext> options) : base(options) { }

    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("grading");

        modelBuilder.Entity<Grade>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Score).HasColumnType("decimal(18,2)");
            e.Property(x => x.MaxScore).HasColumnType("decimal(18,2)");
            e.Property(x => x.Comment).HasMaxLength(1000);
            e.Property(x => x.GradedById).HasMaxLength(450);
            e.HasIndex(x => new { x.CourseId, x.StudentId });
        });
    }
}
