// ITestsDbContext.cs

using Microsoft.EntityFrameworkCore;
using Tests.Domain.Entities;

namespace Tests.Application.Interfaces;

/// <summary>
/// Контракт DbContext фиксирует DbSet-ы и сохранение изменений, доступные application-слою.
/// </summary>
public interface ITestsDbContext
{
    DbSet<Test> Tests { get; }
    DbSet<Question> Questions { get; }
    DbSet<AnswerOption> AnswerOptions { get; }
    DbSet<TestAttempt> TestAttempts { get; }
    DbSet<TestResponse> TestResponses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
