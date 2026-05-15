// Файл: BaseDbContext.cs
using EduPlatform.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Shared.Infrastructure.Persistence;

// DbContext BaseDbContext описывает EF Core-модель и границы хранения данных модуля.
public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
