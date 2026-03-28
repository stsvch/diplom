using Microsoft.EntityFrameworkCore;
using Progress.Domain.Entities;

namespace Progress.Application.Interfaces;

public interface IProgressDbContext
{
    DbSet<LessonProgress> LessonProgresses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
