using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;
using Tests.Domain.Enums;

namespace Tests.Infrastructure.Persistence;

public class TestsDbContext : BaseDbContext, ITestsDbContext
{
    public TestsDbContext(DbContextOptions<TestsDbContext> options) : base(options)
    {
    }

    public DbSet<Test> Tests => Set<Test>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<TestAttempt> TestAttempts => Set<TestAttempt>();
    public DbSet<TestResponse> TestResponses => Set<TestResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("tests");

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(5000);
            entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.CreatedById);
            entity.HasIndex(e => e.CourseId);

            entity.HasMany(e => e.Questions)
                  .WithOne(q => q.Test)
                  .HasForeignKey(q => q.TestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.GradeType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(QuestionGradeType.Auto);
            entity.Property(e => e.Explanation).HasMaxLength(2000);
            entity.Property(e => e.ExpectedAnswer).HasMaxLength(10000);
            entity.HasIndex(e => e.TestId);

            entity.HasMany(e => e.AnswerOptions)
                  .WithOne(a => a.Question)
                  .HasForeignKey(a => a.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.MatchingPairValue).HasMaxLength(2000);
            entity.HasIndex(e => e.QuestionId);
        });

        modelBuilder.Entity<TestAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.TestId);
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => new { e.TestId, e.StudentId });

            entity.HasOne(e => e.Test)
                  .WithMany()
                  .HasForeignKey(e => e.TestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Responses)
                  .WithOne(r => r.Attempt)
                  .HasForeignKey(r => r.AttemptId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TestResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SelectedOptionIds).HasColumnType("text");
            entity.Property(e => e.TextAnswer).HasMaxLength(10000);
            entity.Property(e => e.TeacherComment).HasMaxLength(5000);
            entity.HasIndex(e => e.AttemptId);
            entity.HasIndex(e => new { e.AttemptId, e.QuestionId }).IsUnique();

            entity.HasOne(e => e.Question)
                  .WithMany()
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
