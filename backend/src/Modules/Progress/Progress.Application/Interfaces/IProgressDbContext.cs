using Microsoft.EntityFrameworkCore;
using Progress.Domain.Entities;

namespace Progress.Application.Interfaces;

public interface IProgressDbContext
{
    DbSet<LessonProgress> LessonProgresses { get; }
    DbSet<CourseItemProgress> CourseItemProgresses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
