using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Tools.Application.Interfaces;
using Tools.Domain.Entities;
using Tools.Domain.Enums;

namespace Tools.Infrastructure.Persistence;

public class ToolsDbContext : BaseDbContext, IToolsDbContext
{
    public ToolsDbContext(DbContextOptions<ToolsDbContext> options) : base(options)
    {
    }

    public DbSet<DictionaryWord> DictionaryWords => Set<DictionaryWord>();
    public DbSet<UserDictionaryProgress> UserDictionaryProgress => Set<UserDictionaryProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("tools");

        modelBuilder.Entity<DictionaryWord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CourseId).IsRequired();
            entity.Property(e => e.Term).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Translation).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Definition).HasMaxLength(4000);
            entity.Property(e => e.Example).HasMaxLength(4000);
            entity.Property(e => e.Tags).HasMaxLength(1000);
            entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => new { e.CourseId, e.Term });
            entity.HasIndex(e => e.CreatedById);
        });

        modelBuilder.Entity<UserDictionaryProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WordId).IsRequired();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.IsKnown).IsRequired();
            entity.Property(e => e.ReviewCount).IsRequired();
            entity.Property(e => e.HardCount).IsRequired();
            entity.Property(e => e.RepeatLaterCount).IsRequired();
            entity.Property(e => e.LastOutcome).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.NextReviewAt);
            entity.HasOne(e => e.Word)
                .WithMany(w => w.ProgressEntries)
                .HasForeignKey(e => e.WordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.WordId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.NextReviewAt);
        });
    }
}
