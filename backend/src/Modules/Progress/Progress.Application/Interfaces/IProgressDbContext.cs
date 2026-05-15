// Файл: IProgressDbContext.cs
using Microsoft.EntityFrameworkCore;
using Progress.Domain.Entities;

namespace Progress.Application.Interfaces;

// DbContext IProgressDbContext описывает EF Core-модель и границы хранения данных модуля.
public interface IProgressDbContext
{
    DbSet<LessonProgress> LessonProgresses { get; }
    DbSet<CourseItemProgress> CourseItemProgresses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
