using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence;

public class SchedulingDbContext : BaseDbContext, ISchedulingDbContext
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options) { }

    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();
    public DbSet<SessionBooking> SessionBookings => Set<SessionBooking>();
    public DbSet<TeacherAvailability> TeacherAvailabilities => Set<TeacherAvailability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("scheduling");

        modelBuilder.Entity<ScheduleSlot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TeacherId).IsRequired().HasMaxLength(450);
            e.Property(x => x.TeacherName).IsRequired().HasMaxLength(500);
            e.Property(x => x.Title).IsRequired().HasMaxLength(500);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.MeetingLink).HasMaxLength(2000);
            e.Property(x => x.SessionType).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => new { x.TeacherId, x.StartTime });
            e.HasIndex(x => new { x.AvailabilityId, x.StartTime });
            e.HasMany(x => x.Bookings)
                .WithOne(b => b.Slot)
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SessionBooking>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).IsRequired().HasMaxLength(450);
            e.Property(x => x.StudentName).IsRequired().HasMaxLength(500);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => new { x.StudentId, x.StartTime });
            e.HasIndex(x => new { x.SlotId, x.StudentId }).IsUnique();
        });

        modelBuilder.Entity<TeacherAvailability>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TeacherId).IsRequired().HasMaxLength(450);
            e.Property(x => x.TeacherName).IsRequired().HasMaxLength(500);
            e.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.SessionType).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Title).IsRequired().HasMaxLength(500);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.MeetingLink).HasMaxLength(2000);
            e.HasIndex(x => new { x.TeacherId, x.IsActive });
            e.HasIndex(x => new { x.RequiredCourseId, x.IsActive });
        });
    }
}
