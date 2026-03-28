using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Persistence;

public class NotificationsDbContext : BaseDbContext, INotificationsDbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("notifications");

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Title).IsRequired().HasMaxLength(500);
            e.Property(x => x.Message).IsRequired();
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.LinkUrl).HasMaxLength(1000);
        });
    }
}
