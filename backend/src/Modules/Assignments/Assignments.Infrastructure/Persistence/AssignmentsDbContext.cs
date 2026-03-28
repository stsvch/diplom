using Assignments.Application.Interfaces;
using Assignments.Domain.Entities;
using Assignments.Domain.Enums;
using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Infrastructure.Persistence;

public class AssignmentsDbContext : BaseDbContext, IAssignmentsDbContext
{
    public AssignmentsDbContext(DbContextOptions<AssignmentsDbContext> options) : base(options) { }

    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("assignments");

        modelBuilder.Entity<Assignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.CreatedById).IsRequired().HasMaxLength(450);
            e.HasMany(x => x.Submissions).WithOne(x => x.Assignment).HasForeignKey(x => x.AssignmentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AssignmentSubmission>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => new { x.AssignmentId, x.StudentId, x.AttemptNumber }).IsUnique();
        });
    }
}
