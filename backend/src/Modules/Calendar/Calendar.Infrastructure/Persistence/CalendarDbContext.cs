using Calendar.Application.Interfaces;
using Calendar.Domain.Entities;
using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence;

public class CalendarDbContext : BaseDbContext, ICalendarDbContext
{
    public CalendarDbContext(DbContextOptions<CalendarDbContext> options) : base(options) { }

    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("calendar");

        modelBuilder.Entity<CalendarEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(500);
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.SourceType).HasMaxLength(100);
            e.Property(x => x.EventTime).HasMaxLength(10);
        });
    }
}
