using Grading.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Interfaces;

public interface IGradingDbContext
{
    DbSet<Grade> Grades { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
