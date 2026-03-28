using Microsoft.EntityFrameworkCore;
using Tests.Domain.Entities;

namespace Tests.Application.Interfaces;

public interface ITestsDbContext
{
    DbSet<Test> Tests { get; }
    DbSet<Question> Questions { get; }
    DbSet<AnswerOption> AnswerOptions { get; }
    DbSet<TestAttempt> TestAttempts { get; }
    DbSet<TestResponse> TestResponses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
