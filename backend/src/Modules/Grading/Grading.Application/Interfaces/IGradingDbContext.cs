// IGradingDbContext.cs

using Grading.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Interfaces;

/// <summary>
/// Контракт DbContext фиксирует DbSet-ы и сохранение изменений, доступные application-слою.
/// </summary>
public interface IGradingDbContext
{
    DbSet<Grade> Grades { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
