using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using Courses.Domain.Enums;
using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Courses.Infrastructure.Persistence;

public class CoursesDbContext : BaseDbContext, ICoursesDbContext
{
    public CoursesDbContext(DbContextOptions<CoursesDbContext> options) : base(options)
    {
    }

    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> CourseModules => Set<CourseModule>();
    public DbSet<CourseItem> CourseItems => Set<CourseItem>();
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("courses");

        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.HasMany(e => e.Courses)
                  .WithOne(c => c.Discipline)
                  .HasForeignKey(c => c.DisciplineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.TeacherId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.TeacherName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(1000);
            entity.Property(e => e.Level).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.OrderType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.HasCertificate).HasDefaultValue(false);
            entity.Property(e => e.RatingAverage);
            entity.Property(e => e.RatingCount).HasDefaultValue(0);
            entity.Property(e => e.ReviewsCount).HasDefaultValue(0);
            entity.HasIndex(e => e.TeacherId);
            entity.HasIndex(e => e.DisciplineId);

            entity.HasMany(e => e.Modules)
                  .WithOne(m => m.Course)
                  .HasForeignKey(m => m.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Items)
                  .WithOne(i => i.Course)
                  .HasForeignKey(i => i.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Enrollments)
                  .WithOne(e => e.Course)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Reviews)
                  .WithOne(r => r.Course)
                  .HasForeignKey(r => r.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CourseModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.CourseId);

            entity.HasMany(e => e.Lessons)
                  .WithOne(l => l.Module)
                  .HasForeignKey(l => l.ModuleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Items)
                  .WithOne(i => i.Module)
                  .HasForeignKey(i => i.ModuleId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CourseItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Url).HasMaxLength(2000);
            entity.Property(e => e.ResourceKind).HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.IsRequired).HasDefaultValue(true);
            entity.Property(e => e.Points).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.ModuleId);
            entity.HasIndex(e => new { e.CourseId, e.ModuleId, e.OrderIndex });
            entity.HasIndex(e => new { e.Type, e.SourceId }).IsUnique();
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Layout).HasConversion<string>().HasMaxLength(30);
            entity.HasIndex(e => e.ModuleId);
        });

        modelBuilder.Entity<CourseEnrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => new { e.CourseId, e.StudentId });
            entity.HasIndex(e => e.StudentId);
        });

        modelBuilder.Entity<CourseReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.StudentName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Comment).HasMaxLength(5000);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => new { e.CourseId, e.StudentId }).IsUnique();
        });
    }
}
