// IAssignmentsDbContext.cs

using Assignments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Interfaces;

/// <summary>
/// Контракт Assignments DbContext: открывает задания, сдачи и критерии для handlers/read services и сохраняет изменения.
/// </summary>
public interface IAssignmentsDbContext
{
    DbSet<Assignment> Assignments { get; }
    DbSet<AssignmentSubmission> AssignmentSubmissions { get; }
    DbSet<AssignmentCriteria> AssignmentCriteria { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
