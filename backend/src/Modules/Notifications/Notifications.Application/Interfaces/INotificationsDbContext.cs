// Файл: INotificationsDbContext.cs
using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;

namespace Notifications.Application.Interfaces;

// DbContext INotificationsDbContext описывает EF Core-модель и границы хранения данных модуля.
public interface INotificationsDbContext
{
    DbSet<Notification> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
