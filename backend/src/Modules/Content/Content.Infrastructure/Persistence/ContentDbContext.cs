using System.Text.Json;
using Content.Application.Interfaces;
using Content.Domain.Entities;
using Content.Domain.ValueObjects.Answers;
using Content.Domain.ValueObjects.Blocks;
using Content.Infrastructure.Persistence.JsonConverters;
using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Content.Infrastructure.Persistence;

public class ContentDbContext : BaseDbContext, IContentDbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<LessonBlock> LessonBlocks => Set<LessonBlock>();
    public DbSet<LessonBlockAttempt> LessonBlockAttempts => Set<LessonBlockAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("content");

        var jsonOptions = ContentJsonOptions.Default;

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FileSize);
            entity.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(100);
            entity.Property(e => e.UploadedById).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.UploadedById);
        });

        modelBuilder.Entity<LessonBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LessonId).IsRequired();
            entity.Property(e => e.OrderIndex).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);

            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<LessonBlockData>(v, jsonOptions)!);

            entity.Property(e => e.Settings)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<LessonBlockSettings>(v, jsonOptions)!);

            entity.HasIndex(e => new { e.LessonId, e.OrderIndex });
            entity.HasIndex(e => e.Type);
        });

        modelBuilder.Entity<LessonBlockAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BlockId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();

            entity.Property(e => e.Answers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<LessonBlockAnswer>(v, jsonOptions)!);

            entity.Property(e => e.Score).HasColumnType("decimal(5,2)");
            entity.Property(e => e.MaxScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.ReviewerComment).HasMaxLength(4000);

            entity.HasOne(e => e.Block)
                .WithMany()
                .HasForeignKey(e => e.BlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.BlockId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
        });
    }
}
