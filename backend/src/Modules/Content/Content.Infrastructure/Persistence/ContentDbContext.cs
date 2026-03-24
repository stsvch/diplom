using Content.Application.Interfaces;
using Content.Domain.Entities;
using Content.Domain.Enums;
using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Content.Infrastructure.Persistence;

public class ContentDbContext : BaseDbContext, IContentDbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("content");

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
    }
}
