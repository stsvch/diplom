using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;

namespace Notifications.Application.Interfaces;

public interface INotificationsDbContext
{
    DbSet<Notification> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
