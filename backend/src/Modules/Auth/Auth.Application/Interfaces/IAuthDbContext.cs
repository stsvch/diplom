using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Interfaces;

public interface IAuthDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PlatformSetting> PlatformSettings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
