// IAuthDbContext.cs
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Interfaces;

// Контракт Auth DbContext для application-слоя: даёт доступ к refresh-токенам и singleton-настройкам без зависимости от инфраструктурного DbContext.
public interface IAuthDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PlatformSetting> PlatformSettings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
